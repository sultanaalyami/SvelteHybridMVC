#pragma once

// تعريفات مساعدة لـ eBPF
// مستوحاة من bpf_helper_defs.h

typedef unsigned char uint8_t;
typedef unsigned short uint16_t;
typedef unsigned int uint32_t;
typedef unsigned long long uint64_t;

#define SEC(name) __attribute__((section(name), used))

// أنواع الخرائط
#define BPF_MAP_TYPE_HASH 1
#define BPF_MAP_TYPE_ARRAY 2

// أنواع البرامج
#define BPF_PROG_TYPE_XDP 6

// قيم إرجاع XDP
#define XDP_ABORTED 0
#define XDP_DROP 1
#define XDP_PASS 2
#define XDP_TX 3
#define XDP_REDIRECT 4

// دوال مساعدة
static void *(*bpf_map_lookup_elem)(void *map, const void *key) = (void *)1;
static int (*bpf_map_update_elem)(void *map, const void *key, const void *value, uint64_t flags) = (void *)2;
static int (*bpf_map_delete_elem)(void *map, const void *key) = (void *)3;
static uint64_t (*bpf_ktime_get_ns)(void) = (void *)5;
static int (*bpf_trace_printk)(const char *fmt, int fmt_size, ...) = (void *)6;

// تعريفات الخرائط
struct bpf_map_def {
    unsigned int type;
    unsigned int key_size;
    unsigned int value_size;
    unsigned int max_entries;
    unsigned int map_flags;
};

// تحويلات Endian
#define bpf_htons(x) __builtin_bswap16(x)
#define bpf_ntohs(x) __builtin_bswap16(x)
#define bpf_htonl(x) __builtin_bswap32(x)
#define bpf_ntohl(x) __builtin_bswap32(x)
