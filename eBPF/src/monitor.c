#include "bpf_helpers.h"

// تعريف هياكل البيانات للشبكة (محاكاة لـ net/ip.h)
struct ethhdr {
    unsigned char h_dest[6];
    unsigned char h_source[6];
    unsigned short h_proto;
};

struct iphdr {
    unsigned char ihl : 4;
    unsigned char version : 4;
    unsigned char tos;
    unsigned short tot_len;
    unsigned short id;
    unsigned short frag_off;
    unsigned char ttl;
    unsigned char protocol;
    unsigned short check;
    unsigned int saddr;
    unsigned int daddr;
};

struct udphdr {
    unsigned short source;
    unsigned short dest;
    unsigned short len;
    unsigned short check;
};

struct tcphdr {
    unsigned short source;
    unsigned short dest;
    unsigned int seq;
    unsigned int ack_seq;
    unsigned short res1 : 4;
    unsigned short doff : 4;
    unsigned short fin : 1;
    unsigned short syn : 1;
    unsigned short rst : 1;
    unsigned short psh : 1;
    unsigned short ack : 1;
    unsigned short urg : 1;
    unsigned short ece : 1;
    unsigned short cwr : 1;
    unsigned short window;
    unsigned short check;
    unsigned short urg_ptr;
};

// سياق XDP
struct xdp_md {
    uint32_t data;
    uint32_t data_end;
    uint32_t data_meta;
    uint32_t ingress_ifindex;
    uint32_t rx_queue_index;
};

// 1. تعريف هياكل البيانات للخريطة
struct flow_key {
    uint32_t src_ip;
    uint32_t dst_ip;
    uint16_t src_port;
    uint16_t dst_port;
    uint8_t protocol;
};

struct flow_stats {
    uint64_t packets;
    uint64_t bytes;
    uint64_t last_seen;
};

// تعريف الخريطة: BPF_MAP_TYPE_HASH
struct {
    __uint(type, BPF_MAP_TYPE_HASH);
    __type(key, struct flow_key);
    __type(value, struct flow_stats);
    __uint(max_entries, 1024);
} flow_stats_map SEC(".maps");

// 2. برنامج النواة (Kernel-Space Program)
SEC("xdp")
int monitor_traffic(struct xdp_md *ctx) {
    void *data_end = (void *)(long)ctx->data_end;
    void *data = (void *)(long)ctx->data;

    // تحليل Ethernet Header
    struct ethhdr *eth = data;
    if ((void *)(eth + 1) > data_end)
        return XDP_PASS;

    // التحقق من IPv4 (0x0800)
    if (eth->h_proto != bpf_htons(0x0800))
        return XDP_PASS;

    // تحليل IP Header
    struct iphdr *ip = (struct iphdr *)(eth + 1);
    if ((void *)(ip + 1) > data_end)
        return XDP_PASS;

    struct flow_key key = {};
    key.src_ip = ip->saddr;
    key.dst_ip = ip->daddr;
    key.protocol = ip->protocol;

    // تحليل Transport Header (TCP/UDP)
    if (ip->protocol == 6) { // TCP
        struct tcphdr *tcp = (struct tcphdr *)(ip + 1);
        if ((void *)(tcp + 1) > data_end)
            return XDP_PASS;
        key.src_port = tcp->source;
        key.dst_port = tcp->dest;
    } else if (ip->protocol == 17) { // UDP
        struct udphdr *udp = (struct udphdr *)(ip + 1);
        if ((void *)(udp + 1) > data_end)
            return XDP_PASS;
        key.src_port = udp->source;
        key.dst_port = udp->dest;
    } else {
        return XDP_PASS;
    }

    // تحديث الإحصائيات في الخريطة
    struct flow_stats *stats = bpf_map_lookup_elem(&flow_stats_map, &key);
    if (stats) {
        __sync_fetch_and_add(&stats->packets, 1);
        __sync_fetch_and_add(&stats->bytes, bpf_ntohs(ip->tot_len));
        stats->last_seen = bpf_ktime_get_ns();
    } else {
        struct flow_stats new_stats = {};
        new_stats.packets = 1;
        new_stats.bytes = bpf_ntohs(ip->tot_len);
        new_stats.last_seen = bpf_ktime_get_ns();
        bpf_map_update_elem(&flow_stats_map, &key, &new_stats, 0);
    }

    return XDP_PASS;
}
