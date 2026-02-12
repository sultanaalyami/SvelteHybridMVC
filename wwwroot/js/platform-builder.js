/**
 * HRCE Platform Builder
 * Professional No-Code Platform Builder with Drag & Drop
 * @version 1.0.0
 */

(function () {
    'use strict';

    // ???????????????????????????????????????????????????????????????????????????
    // Configuration & State
    // ???????????????????????????????????????????????????????????????????????????
    const CONFIG = {
        autoSaveInterval: 30000,
        maxHistorySteps: 50,
        defaultZoom: 100,
        minZoom: 25,
        maxZoom: 200,
        zoomStep: 25
    };

    const state = {
        project: {
            id: generateId(),
            name: '„‘—Ê⁄ ÃœÌœ',
            pages: [{
                id: 'home',
                name: '«·—∆Ì”Ì…',
                elements: []
            }],
            currentPage: 'home',
            styles: {},
            settings: {}
        },
        ui: {
            zoom: 100,
            view: 'desktop',
            selectedElement: null,
            clipboard: null,
            isDragging: false,
            draggedComponent: null
        },
        history: {
            past: [],
            future: []
        }
    };

    // ???????????????????????????????????????????????????????????????????????????
    // Component Templates
    // ???????????????????????????????????????????????????????????????????????????
    const componentTemplates = {
        // Layout Components
        container: {
            tag: 'div',
            defaultStyles: {
                padding: '24px',
                backgroundColor: 'transparent'
            },
            defaultContent: '',
            canHaveChildren: true,
            category: 'layout'
        },
        row: {
            tag: 'div',
            defaultStyles: {
                display: 'flex',
                flexDirection: 'row',
                gap: '16px',
                padding: '16px'
            },
            defaultContent: '',
            canHaveChildren: true,
            category: 'layout'
        },
        columns: {
            tag: 'div',
            defaultStyles: {
                display: 'grid',
                gridTemplateColumns: 'repeat(2, 1fr)',
                gap: '24px',
                padding: '16px'
            },
            defaultContent: '',
            canHaveChildren: true,
            category: 'layout'
        },
        grid: {
            tag: 'div',
            defaultStyles: {
                display: 'grid',
                gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))',
                gap: '16px',
                padding: '16px'
            },
            defaultContent: '',
            canHaveChildren: true,
            category: 'layout'
        },
        section: {
            tag: 'section',
            defaultStyles: {
                padding: '48px 24px',
                backgroundColor: '#161b22'
            },
            defaultContent: '',
            canHaveChildren: true,
            category: 'layout'
        },

        // Navigation Components
        navbar: {
            tag: 'nav',
            defaultStyles: {
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'space-between',
                padding: '16px 24px',
                backgroundColor: '#0d1117',
                borderBottom: '1px solid rgba(240, 246, 252, 0.1)'
            },
            defaultContent: `
                <div style="font-weight: 600; color: #f0f6fc; font-size: 18px;">«·‘⁄«—</div>
                <div style="display: flex; gap: 24px;">
                    <a href="#" style="color: #c9d1d9; text-decoration: none;">«·—∆Ì”Ì…</a>
                    <a href="#" style="color: #8b949e; text-decoration: none;">«·Œœ„« </a>
                    <a href="#" style="color: #8b949e; text-decoration: none;">„‰ ‰Õ‰</a>
                    <a href="#" style="color: #8b949e; text-decoration: none;"> Ê«’· „⁄‰«</a>
                </div>
            `,
            canHaveChildren: false,
            category: 'navigation'
        },
        sidebar: {
            tag: 'aside',
            defaultStyles: {
                width: '260px',
                minHeight: '400px',
                padding: '20px',
                backgroundColor: '#161b22',
                borderRadius: '8px'
            },
            defaultContent: `
                <div style="font-weight: 600; color: #f0f6fc; margin-bottom: 20px;">«·ﬁ«∆„…</div>
                <div style="display: flex; flex-direction: column; gap: 8px;">
                    <a href="#" style="padding: 10px 16px; background: rgba(88,166,255,0.15); color: #58a6ff; text-decoration: none; border-radius: 6px;">·ÊÕ… «· Õﬂ„</a>
                    <a href="#" style="padding: 10px 16px; color: #8b949e; text-decoration: none; border-radius: 6px;">«·„” Œœ„Ê‰</a>
                    <a href="#" style="padding: 10px 16px; color: #8b949e; text-decoration: none; border-radius: 6px;">«·≈⁄œ«œ« </a>
                </div>
            `,
            canHaveChildren: false,
            category: 'navigation'
        },
        tabs: {
            tag: 'div',
            defaultStyles: {
                backgroundColor: '#161b22',
                borderRadius: '8px',
                overflow: 'hidden'
            },
            defaultContent: `
                <div style="display: flex; border-bottom: 1px solid rgba(240,246,252,0.1);">
                    <button style="padding: 12px 20px; background: #1c2128; color: #f0f6fc; border: none; cursor: pointer;"> »ÊÌ» 1</button>
                    <button style="padding: 12px 20px; background: transparent; color: #8b949e; border: none; cursor: pointer;"> »ÊÌ» 2</button>
                    <button style="padding: 12px 20px; background: transparent; color: #8b949e; border: none; cursor: pointer;"> »ÊÌ» 3</button>
                </div>
                <div style="padding: 20px; color: #c9d1d9;">„Õ ÊÏ «· »ÊÌ» «·√Ê·</div>
            `,
            canHaveChildren: false,
            category: 'navigation'
        },
        breadcrumb: {
            tag: 'nav',
            defaultStyles: {
                padding: '12px 0'
            },
            defaultContent: `
                <div style="display: flex; align-items: center; gap: 8px; color: #8b949e; font-size: 14px;">
                    <a href="#" style="color: #58a6ff; text-decoration: none;">«·—∆Ì”Ì…</a>
                    <span>/</span>
                    <a href="#" style="color: #58a6ff; text-decoration: none;">«·ﬁ”„</a>
                    <span>/</span>
                    <span style="color: #c9d1d9;">«·’›Õ… «·Õ«·Ì…</span>
                </div>
            `,
            canHaveChildren: false,
            category: 'navigation'
        },
        menu: {
            tag: 'div',
            defaultStyles: {
                backgroundColor: '#161b22',
                borderRadius: '8px',
                padding: '8px',
                minWidth: '180px'
            },
            defaultContent: `
                <a href="#" style="display: block; padding: 10px 16px; color: #c9d1d9; text-decoration: none; border-radius: 4px;">⁄‰’— 1</a>
                <a href="#" style="display: block; padding: 10px 16px; color: #c9d1d9; text-decoration: none; border-radius: 4px;">⁄‰’— 2</a>
                <a href="#" style="display: block; padding: 10px 16px; color: #c9d1d9; text-decoration: none; border-radius: 4px;">⁄‰’— 3</a>
            `,
            canHaveChildren: false,
            category: 'navigation'
        },
        footer: {
            tag: 'footer',
            defaultStyles: {
                padding: '40px 24px',
                backgroundColor: '#0d1117',
                borderTop: '1px solid rgba(240, 246, 252, 0.1)',
                textAlign: 'center'
            },
            defaultContent: `
                <p style="color: #8b949e; margin: 0;">© 2025 ‘—ﬂ ﬂ. Ã„Ì⁄ «·ÕﬁÊﬁ „Õ›ÊŸ….</p>
            `,
            canHaveChildren: false,
            category: 'navigation'
        },

        // Content Components
        heading: {
            tag: 'h2',
            defaultStyles: {
                fontSize: '32px',
                fontWeight: '700',
                color: '#f0f6fc',
                marginBottom: '16px'
            },
            defaultContent: '⁄‰Ê«‰ —∆Ì”Ì',
            editable: true,
            canHaveChildren: false,
            category: 'content'
        },
        paragraph: {
            tag: 'p',
            defaultStyles: {
                fontSize: '16px',
                lineHeight: '1.7',
                color: '#c9d1d9'
            },
            defaultContent: 'Â–« ‰’  Ã—Ì»Ì Ì„ﬂ‰ﬂ  ⁄œÌ·Â. √÷› «·„Õ ÊÏ «·Œ«’ »ﬂ Â‰«.',
            editable: true,
            canHaveChildren: false,
            category: 'content'
        },
        image: {
            tag: 'img',
            defaultStyles: {
                maxWidth: '100%',
                height: 'auto',
                borderRadius: '8px'
            },
            defaultAttributes: {
                src: 'https://via.placeholder.com/800x400/1c2128/58a6ff?text=’Ê—…',
                alt: 'Ê’› «·’Ê—…'
            },
            canHaveChildren: false,
            category: 'content'
        },
        video: {
            tag: 'div',
            defaultStyles: {
                position: 'relative',
                paddingBottom: '56.25%',
                backgroundColor: '#0d1117',
                borderRadius: '8px',
                overflow: 'hidden'
            },
            defaultContent: `
                <div style="position: absolute; top: 50%; left: 50%; transform: translate(-50%, -50%); text-align: center; color: #8b949e;">
                    <svg width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                        <polygon points="5 3 19 12 5 21 5 3"></polygon>
                    </svg>
                    <p style="margin-top: 8px;">√÷› —«»ÿ «·›ÌœÌÊ</p>
                </div>
            `,
            canHaveChildren: false,
            category: 'content'
        },
        icon: {
            tag: 'div',
            defaultStyles: {
                width: '48px',
                height: '48px',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                backgroundColor: 'rgba(88, 166, 255, 0.15)',
                borderRadius: '12px',
                color: '#58a6ff'
            },
            defaultContent: `
                <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                    <polygon points="12 2 15.09 8.26 22 9.27 17 14.14 18.18 21.02 12 17.77 5.82 21.02 7 14.14 2 9.27 8.91 8.26 12 2"></polygon>
                </svg>
            `,
            canHaveChildren: false,
            category: 'content'
        },
        divider: {
            tag: 'hr',
            defaultStyles: {
                border: 'none',
                borderTop: '1px solid rgba(240, 246, 252, 0.1)',
                margin: '24px 0'
            },
            canHaveChildren: false,
            category: 'content'
        },
        spacer: {
            tag: 'div',
            defaultStyles: {
                height: '40px'
            },
            canHaveChildren: false,
            category: 'content'
        },

        // Data Display Components
        card: {
            tag: 'div',
            defaultStyles: {
                backgroundColor: '#161b22',
                border: '1px solid rgba(240, 246, 252, 0.1)',
                borderRadius: '12px',
                padding: '24px',
                transition: 'transform 0.2s, box-shadow 0.2s'
            },
            defaultContent: `
                <h3 style="font-size: 18px; font-weight: 600; color: #f0f6fc; margin-bottom: 12px;">⁄‰Ê«‰ «·»ÿ«ﬁ…</h3>
                <p style="color: #8b949e; font-size: 14px; line-height: 1.6;">Ê’› „Œ ’— ··»ÿ«ﬁ… ÌÊ÷Õ «·„Õ ÊÏ √Ê «·Œœ„… «·„ﬁœ„….</p>
            `,
            canHaveChildren: true,
            category: 'data'
        },
        'stat-card': {
            tag: 'div',
            defaultStyles: {
                backgroundColor: '#161b22',
                border: '1px solid rgba(240, 246, 252, 0.1)',
                borderRadius: '12px',
                padding: '20px'
            },
            defaultContent: `
                <div style="font-size: 12px; color: #8b949e; text-transform: uppercase; letter-spacing: 0.05em; margin-bottom: 8px;">≈Ã„«·Ì «·„»Ì⁄« </div>
                <div style="font-size: 32px; font-weight: 700; color: #f0f6fc;">12,450</div>
                <div style="font-size: 13px; color: #3fb950; margin-top: 8px;">? 12% „‰ «·‘Â— «·„«÷Ì</div>
            `,
            canHaveChildren: false,
            category: 'data'
        },
        table: {
            tag: 'div',
            defaultStyles: {
                backgroundColor: '#161b22',
                border: '1px solid rgba(240, 246, 252, 0.1)',
                borderRadius: '12px',
                overflow: 'hidden'
            },
            defaultContent: `
                <table style="width: 100%; border-collapse: collapse;">
                    <thead>
                        <tr style="background: #1c2128;">
                            <th style="padding: 12px 16px; text-align: right; font-size: 12px; font-weight: 600; color: #8b949e; text-transform: uppercase;">«·«”„</th>
                            <th style="padding: 12px 16px; text-align: right; font-size: 12px; font-weight: 600; color: #8b949e; text-transform: uppercase;">«·»—Ìœ</th>
                            <th style="padding: 12px 16px; text-align: right; font-size: 12px; font-weight: 600; color: #8b949e; text-transform: uppercase;">«·Õ«·…</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr style="border-top: 1px solid rgba(240,246,252,0.06);">
                            <td style="padding: 14px 16px; color: #c9d1d9;">√Õ„œ „Õ„œ</td>
                            <td style="padding: 14px 16px; color: #8b949e;">ahmed@example.com</td>
                            <td style="padding: 14px 16px;"><span style="padding: 4px 8px; background: rgba(63,185,80,0.15); color: #3fb950; border-radius: 4px; font-size: 12px;">‰‘ÿ</span></td>
                        </tr>
                        <tr style="border-top: 1px solid rgba(240,246,252,0.06);">
                            <td style="padding: 14px 16px; color: #c9d1d9;">”«—… ⁄·Ì</td>
                            <td style="padding: 14px 16px; color: #8b949e;">sara@example.com</td>
                            <td style="padding: 14px 16px;"><span style="padding: 4px 8px; background: rgba(210,153,34,0.15); color: #d29922; border-radius: 4px; font-size: 12px;">„⁄·ﬁ</span></td>
                        </tr>
                    </tbody>
                </table>
            `,
            canHaveChildren: false,
            category: 'data'
        },
        list: {
            tag: 'ul',
            defaultStyles: {
                listStyle: 'none',
                padding: '0',
                margin: '0'
            },
            defaultContent: `
                <li style="display: flex; align-items: center; gap: 12px; padding: 12px 0; border-bottom: 1px solid rgba(240,246,252,0.06);">
                    <span style="width: 8px; height: 8px; background: #58a6ff; border-radius: 50%;"></span>
                    <span style="color: #c9d1d9;">«·⁄‰’— «·√Ê·</span>
                </li>
                <li style="display: flex; align-items: center; gap: 12px; padding: 12px 0; border-bottom: 1px solid rgba(240,246,252,0.06);">
                    <span style="width: 8px; height: 8px; background: #58a6ff; border-radius: 50%;"></span>
                    <span style="color: #c9d1d9;">«·⁄‰’— «·À«‰Ì</span>
                </li>
                <li style="display: flex; align-items: center; gap: 12px; padding: 12px 0;">
                    <span style="width: 8px; height: 8px; background: #58a6ff; border-radius: 50%;"></span>
                    <span style="color: #c9d1d9;">«·⁄‰’— «·À«·À</span>
                </li>
            `,
            canHaveChildren: false,
            category: 'data'
        },
        progress: {
            tag: 'div',
            defaultStyles: {},
            defaultContent: `
                <div style="display: flex; justify-content: space-between; margin-bottom: 8px;">
                    <span style="font-size: 14px; color: #c9d1d9;">«· ﬁœ„</span>
                    <span style="font-size: 14px; color: #8b949e;">75%</span>
                </div>
                <div style="height: 8px; background: #21262d; border-radius: 4px; overflow: hidden;">
                    <div style="width: 75%; height: 100%; background: linear-gradient(90deg, #58a6ff, #a371f7); border-radius: 4px;"></div>
                </div>
            `,
            canHaveChildren: false,
            category: 'data'
        },
        badge: {
            tag: 'span',
            defaultStyles: {
                display: 'inline-flex',
                alignItems: 'center',
                padding: '4px 12px',
                backgroundColor: 'rgba(88, 166, 255, 0.15)',
                color: '#58a6ff',
                fontSize: '12px',
                fontWeight: '600',
                borderRadius: '999px'
            },
            defaultContent: '‘«—…',
            editable: true,
            canHaveChildren: false,
            category: 'data'
        },
        avatar: {
            tag: 'div',
            defaultStyles: {
                width: '48px',
                height: '48px',
                borderRadius: '50%',
                backgroundColor: '#1c2128',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                color: '#8b949e',
                fontSize: '18px',
                fontWeight: '600'
            },
            defaultContent: '√',
            canHaveChildren: false,
            category: 'data'
        },

        // Charts
        'bar-chart': {
            tag: 'div',
            defaultStyles: {
                backgroundColor: '#161b22',
                border: '1px solid rgba(240, 246, 252, 0.1)',
                borderRadius: '12px',
                padding: '24px',
                minHeight: '300px'
            },
            defaultContent: `
                <div style="font-weight: 600; color: #f0f6fc; margin-bottom: 20px;">—”„ »Ì«‰Ì √⁄„œ…</div>
                <div style="display: flex; align-items: flex-end; justify-content: space-around; height: 200px; padding-top: 20px;">
                    <div style="width: 40px; height: 60%; background: linear-gradient(180deg, #58a6ff, #58a6ff80); border-radius: 4px 4px 0 0;"></div>
                    <div style="width: 40px; height: 80%; background: linear-gradient(180deg, #58a6ff, #58a6ff80); border-radius: 4px 4px 0 0;"></div>
                    <div style="width: 40px; height: 45%; background: linear-gradient(180deg, #58a6ff, #58a6ff80); border-radius: 4px 4px 0 0;"></div>
                    <div style="width: 40px; height: 90%; background: linear-gradient(180deg, #58a6ff, #58a6ff80); border-radius: 4px 4px 0 0;"></div>
                    <div style="width: 40px; height: 70%; background: linear-gradient(180deg, #58a6ff, #58a6ff80); border-radius: 4px 4px 0 0;"></div>
                </div>
            `,
            canHaveChildren: false,
            category: 'charts'
        },
        'line-chart': {
            tag: 'div',
            defaultStyles: {
                backgroundColor: '#161b22',
                border: '1px solid rgba(240, 246, 252, 0.1)',
                borderRadius: '12px',
                padding: '24px',
                minHeight: '300px'
            },
            defaultContent: `
                <div style="font-weight: 600; color: #f0f6fc; margin-bottom: 20px;">—”„ »Ì«‰Ì ŒÿÌ</div>
                <svg viewBox="0 0 400 200" style="width: 100%; height: 200px;">
                    <defs>
                        <linearGradient id="lineGradient" x1="0%" y1="0%" x2="0%" y2="100%">
                            <stop offset="0%" style="stop-color:#58a6ff;stop-opacity:0.3" />
                            <stop offset="100%" style="stop-color:#58a6ff;stop-opacity:0" />
                        </linearGradient>
                    </defs>
                    <path d="M 0 150 Q 50 120, 100 100 T 200 80 T 300 60 T 400 40 L 400 200 L 0 200 Z" fill="url(#lineGradient)" />
                    <path d="M 0 150 Q 50 120, 100 100 T 200 80 T 300 60 T 400 40" fill="none" stroke="#58a6ff" stroke-width="3" />
                </svg>
            `,
            canHaveChildren: false,
            category: 'charts'
        },
        'pie-chart': {
            tag: 'div',
            defaultStyles: {
                backgroundColor: '#161b22',
                border: '1px solid rgba(240, 246, 252, 0.1)',
                borderRadius: '12px',
                padding: '24px',
                minHeight: '300px',
                display: 'flex',
                flexDirection: 'column',
                alignItems: 'center'
            },
            defaultContent: `
                <div style="font-weight: 600; color: #f0f6fc; margin-bottom: 20px; align-self: flex-start;">—”„ œ«∆—Ì</div>
                <svg viewBox="0 0 200 200" style="width: 180px; height: 180px;">
                    <circle cx="100" cy="100" r="80" fill="none" stroke="#58a6ff" stroke-width="40" stroke-dasharray="150 502" transform="rotate(-90 100 100)" />
                    <circle cx="100" cy="100" r="80" fill="none" stroke="#a371f7" stroke-width="40" stroke-dasharray="100 502" stroke-dashoffset="-150" transform="rotate(-90 100 100)" />
                    <circle cx="100" cy="100" r="80" fill="none" stroke="#3fb950" stroke-width="40" stroke-dasharray="80 502" stroke-dashoffset="-250" transform="rotate(-90 100 100)" />
                </svg>
            `,
            canHaveChildren: false,
            category: 'charts'
        },
        'area-chart': {
            tag: 'div',
            defaultStyles: {
                backgroundColor: '#161b22',
                border: '1px solid rgba(240, 246, 252, 0.1)',
                borderRadius: '12px',
                padding: '24px',
                minHeight: '300px'
            },
            defaultContent: `
                <div style="font-weight: 600; color: #f0f6fc; margin-bottom: 20px;">—”„ „”«Õ…</div>
                <svg viewBox="0 0 400 200" style="width: 100%; height: 200px;">
                    <defs>
                        <linearGradient id="areaGradient" x1="0%" y1="0%" x2="0%" y2="100%">
                            <stop offset="0%" style="stop-color:#a371f7;stop-opacity:0.5" />
                            <stop offset="100%" style="stop-color:#a371f7;stop-opacity:0" />
                        </linearGradient>
                    </defs>
                    <path d="M 0 180 L 50 140 L 100 160 L 150 100 L 200 120 L 250 80 L 300 90 L 350 50 L 400 70 L 400 200 L 0 200 Z" fill="url(#areaGradient)" />
                    <path d="M 0 180 L 50 140 L 100 160 L 150 100 L 200 120 L 250 80 L 300 90 L 350 50 L 400 70" fill="none" stroke="#a371f7" stroke-width="3" />
                </svg>
            `,
            canHaveChildren: false,
            category: 'charts'
        },

        // Form Components
        form: {
            tag: 'form',
            defaultStyles: {
                backgroundColor: '#161b22',
                border: '1px solid rgba(240, 246, 252, 0.1)',
                borderRadius: '12px',
                padding: '24px'
            },
            defaultContent: '',
            canHaveChildren: true,
            category: 'forms'
        },
        input: {
            tag: 'div',
            defaultStyles: {
                marginBottom: '16px'
            },
            defaultContent: `
                <label style="display: block; font-size: 13px; color: #c9d1d9; margin-bottom: 6px;">Õﬁ· «·≈œŒ«·</label>
                <input type="text" placeholder="√œŒ· «·‰’ Â‰«" style="width: 100%; padding: 10px 14px; background: #0d1117; border: 1px solid rgba(240,246,252,0.1); border-radius: 6px; color: #f0f6fc; font-size: 14px;" />
            `,
            canHaveChildren: false,
            category: 'forms'
        },
        textarea: {
            tag: 'div',
            defaultStyles: {
                marginBottom: '16px'
            },
            defaultContent: `
                <label style="display: block; font-size: 13px; color: #c9d1d9; margin-bottom: 6px;">„‰ÿﬁ… «·‰’</label>
                <textarea placeholder="√œŒ· «·‰’ Â‰«" rows="4" style="width: 100%; padding: 10px 14px; background: #0d1117; border: 1px solid rgba(240,246,252,0.1); border-radius: 6px; color: #f0f6fc; font-size: 14px; resize: vertical;"></textarea>
            `,
            canHaveChildren: false,
            category: 'forms'
        },
        select: {
            tag: 'div',
            defaultStyles: {
                marginBottom: '16px'
            },
            defaultContent: `
                <label style="display: block; font-size: 13px; color: #c9d1d9; margin-bottom: 6px;">ﬁ«∆„… «·«Œ Ì«—</label>
                <select style="width: 100%; padding: 10px 14px; background: #0d1117; border: 1px solid rgba(240,246,252,0.1); border-radius: 6px; color: #f0f6fc; font-size: 14px;">
                    <option>«·ŒÌ«— «·√Ê·</option>
                    <option>«·ŒÌ«— «·À«‰Ì</option>
                    <option>«·ŒÌ«— «·À«·À</option>
                </select>
            `,
            canHaveChildren: false,
            category: 'forms'
        },
        checkbox: {
            tag: 'label',
            defaultStyles: {
                display: 'flex',
                alignItems: 'center',
                gap: '10px',
                cursor: 'pointer',
                marginBottom: '12px'
            },
            defaultContent: `
                <input type="checkbox" style="width: 18px; height: 18px; accent-color: #58a6ff;" />
                <span style="color: #c9d1d9; font-size: 14px;">ŒÌ«— ··«Œ Ì«—</span>
            `,
            canHaveChildren: false,
            category: 'forms'
        },
        radio: {
            tag: 'label',
            defaultStyles: {
                display: 'flex',
                alignItems: 'center',
                gap: '10px',
                cursor: 'pointer',
                marginBottom: '12px'
            },
            defaultContent: `
                <input type="radio" name="radio-group" style="width: 18px; height: 18px; accent-color: #58a6ff;" />
                <span style="color: #c9d1d9; font-size: 14px;">ŒÌ«— —«œÌÊ</span>
            `,
            canHaveChildren: false,
            category: 'forms'
        },
        switch: {
            tag: 'label',
            defaultStyles: {
                display: 'flex',
                alignItems: 'center',
                gap: '12px',
                cursor: 'pointer'
            },
            defaultContent: `
                <div style="position: relative; width: 44px; height: 24px; background: #21262d; border-radius: 12px; transition: background 0.2s;">
                    <div style="position: absolute; top: 2px; right: 2px; width: 20px; height: 20px; background: #8b949e; border-radius: 50%; transition: transform 0.2s;"></div>
                </div>
                <span style="color: #c9d1d9; font-size: 14px;">„› «Õ «· »œÌ·</span>
            `,
            canHaveChildren: false,
            category: 'forms'
        },
        button: {
            tag: 'button',
            defaultStyles: {
                padding: '12px 24px',
                backgroundColor: '#58a6ff',
                color: '#0d1117',
                border: 'none',
                borderRadius: '8px',
                fontSize: '14px',
                fontWeight: '600',
                cursor: 'pointer',
                transition: 'background 0.2s'
            },
            defaultContent: '“— «·≈Ã—«¡',
            editable: true,
            canHaveChildren: false,
            category: 'forms'
        },
        'file-upload': {
            tag: 'div',
            defaultStyles: {
                border: '2px dashed rgba(240, 246, 252, 0.2)',
                borderRadius: '8px',
                padding: '40px 24px',
                textAlign: 'center',
                cursor: 'pointer',
                transition: 'border-color 0.2s'
            },
            defaultContent: `
                <svg width="40" height="40" viewBox="0 0 24 24" fill="none" stroke="#8b949e" stroke-width="2" style="margin-bottom: 12px;">
                    <path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"></path>
                    <polyline points="17 8 12 3 7 8"></polyline>
                    <line x1="12" y1="3" x2="12" y2="15"></line>
                </svg>
                <p style="color: #c9d1d9; margin-bottom: 4px;">«”Õ» «·„·›«  Â‰« √Ê «‰ﬁ— ·· Õ„Ì·</p>
                <p style="color: #8b949e; font-size: 12px;">PNG, JPG Õ Ï 10MB</p>
            `,
            canHaveChildren: false,
            category: 'forms'
        },
        'date-picker': {
            tag: 'div',
            defaultStyles: {
                marginBottom: '16px'
            },
            defaultContent: `
                <label style="display: block; font-size: 13px; color: #c9d1d9; margin-bottom: 6px;">«Œ Ì«— «· «—ÌŒ</label>
                <input type="date" style="width: 100%; padding: 10px 14px; background: #0d1117; border: 1px solid rgba(240,246,252,0.1); border-radius: 6px; color: #f0f6fc; font-size: 14px;" />
            `,
            canHaveChildren: false,
            category: 'forms'
        },

        // Interactive Components
        modal: {
            tag: 'div',
            defaultStyles: {
                backgroundColor: '#161b22',
                border: '1px solid rgba(240, 246, 252, 0.1)',
                borderRadius: '12px',
                padding: '24px',
                maxWidth: '500px',
                boxShadow: '0 16px 48px rgba(0,0,0,0.5)'
            },
            defaultContent: `
                <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 20px;">
                    <h3 style="font-size: 18px; font-weight: 600; color: #f0f6fc; margin: 0;">⁄‰Ê«‰ «·‰«›–…</h3>
                    <button style="background: none; border: none; color: #8b949e; cursor: pointer; font-size: 20px;">◊</button>
                </div>
                <p style="color: #8b949e; margin-bottom: 24px;">„Õ ÊÏ «·‰«›–… «·„‰»Àﬁ… ÌŸÂ— Â‰«.</p>
                <div style="display: flex; gap: 12px; justify-content: flex-end;">
                    <button style="padding: 10px 20px; background: #21262d; color: #c9d1d9; border: none; border-radius: 6px; cursor: pointer;">≈·€«¡</button>
                    <button style="padding: 10px 20px; background: #58a6ff; color: #0d1117; border: none; border-radius: 6px; cursor: pointer;"> √ﬂÌœ</button>
                </div>
            `,
            canHaveChildren: false,
            category: 'interactive'
        },
        accordion: {
            tag: 'div',
            defaultStyles: {
                backgroundColor: '#161b22',
                border: '1px solid rgba(240, 246, 252, 0.1)',
                borderRadius: '12px',
                overflow: 'hidden'
            },
            defaultContent: `
                <div style="border-bottom: 1px solid rgba(240,246,252,0.06);">
                    <button style="width: 100%; display: flex; justify-content: space-between; align-items: center; padding: 16px 20px; background: none; border: none; color: #f0f6fc; cursor: pointer; font-size: 14px; text-align: right;">
                        <span>«·”ƒ«· «·√Ê·</span>
                        <span>?</span>
                    </button>
                    <div style="padding: 0 20px 16px; color: #8b949e; font-size: 14px;">≈Ã«»… «·”ƒ«· «·√Ê·  ŸÂ— Â‰«.</div>
                </div>
                <div>
                    <button style="width: 100%; display: flex; justify-content: space-between; align-items: center; padding: 16px 20px; background: none; border: none; color: #c9d1d9; cursor: pointer; font-size: 14px; text-align: right;">
                        <span>«·”ƒ«· «·À«‰Ì</span>
                        <span>?</span>
                    </button>
                </div>
            `,
            canHaveChildren: false,
            category: 'interactive'
        },
        alert: {
            tag: 'div',
            defaultStyles: {
                display: 'flex',
                alignItems: 'flex-start',
                gap: '12px',
                padding: '16px 20px',
                backgroundColor: 'rgba(88, 166, 255, 0.1)',
                border: '1px solid rgba(88, 166, 255, 0.3)',
                borderRadius: '8px'
            },
            defaultContent: `
                <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="#58a6ff" stroke-width="2" style="flex-shrink: 0; margin-top: 2px;">
                    <circle cx="12" cy="12" r="10"></circle>
                    <line x1="12" y1="16" x2="12" y2="12"></line>
                    <line x1="12" y1="8" x2="12.01" y2="8"></line>
                </svg>
                <div>
                    <div style="font-weight: 600; color: #58a6ff; margin-bottom: 4px;">„⁄·Ê„…</div>
                    <div style="color: #c9d1d9; font-size: 14px;">Â–«  ‰»ÌÂ „⁄·Ê„« Ì Ì„ﬂ‰ﬂ  ⁄œÌ· „Õ Ê«Â.</div>
                </div>
            `,
            canHaveChildren: false,
            category: 'interactive'
        },
        tooltip: {
            tag: 'div',
            defaultStyles: {
                display: 'inline-block',
                position: 'relative'
            },
            defaultContent: `
                <span style="color: #58a6ff; text-decoration: underline dashed; cursor: help;">‰’ „⁄  ·„ÌÕ</span>
                <div style="position: absolute; bottom: 100%; right: 50%; transform: translateX(50%); padding: 8px 12px; background: #1c2128; color: #c9d1d9; font-size: 12px; border-radius: 6px; white-space: nowrap; margin-bottom: 8px; box-shadow: 0 4px 12px rgba(0,0,0,0.3);">
                    Â–« ‰’ «· ·„ÌÕ
                    <div style="position: absolute; top: 100%; right: 50%; transform: translateX(50%); border: 6px solid transparent; border-top-color: #1c2128;"></div>
                </div>
            `,
            canHaveChildren: false,
            category: 'interactive'
        },
        carousel: {
            tag: 'div',
            defaultStyles: {
                position: 'relative',
                backgroundColor: '#161b22',
                borderRadius: '12px',
                overflow: 'hidden'
            },
            defaultContent: `
                <div style="aspect-ratio: 16/9; background: linear-gradient(135deg, #1c2128, #21262d); display: flex; align-items: center; justify-content: center;">
                    <span style="color: #8b949e;">‘—ÌÕ… 1</span>
                </div>
                <div style="display: flex; justify-content: center; gap: 8px; padding: 16px;">
                    <span style="width: 8px; height: 8px; background: #58a6ff; border-radius: 50%;"></span>
                    <span style="width: 8px; height: 8px; background: #8b949e; border-radius: 50%;"></span>
                    <span style="width: 8px; height: 8px; background: #8b949e; border-radius: 50%;"></span>
                </div>
            `,
            canHaveChildren: false,
            category: 'interactive'
        },

        // Media Components
        gallery: {
            tag: 'div',
            defaultStyles: {
                display: 'grid',
                gridTemplateColumns: 'repeat(3, 1fr)',
                gap: '8px'
            },
            defaultContent: `
                <div style="aspect-ratio: 1; background: linear-gradient(135deg, #1c2128, #21262d); border-radius: 8px;"></div>
                <div style="aspect-ratio: 1; background: linear-gradient(135deg, #1c2128, #21262d); border-radius: 8px;"></div>
                <div style="aspect-ratio: 1; background: linear-gradient(135deg, #1c2128, #21262d); border-radius: 8px;"></div>
                <div style="aspect-ratio: 1; background: linear-gradient(135deg, #1c2128, #21262d); border-radius: 8px;"></div>
                <div style="aspect-ratio: 1; background: linear-gradient(135deg, #1c2128, #21262d); border-radius: 8px;"></div>
                <div style="aspect-ratio: 1; background: linear-gradient(135deg, #1c2128, #21262d); border-radius: 8px;"></div>
            `,
            canHaveChildren: false,
            category: 'media'
        },
        map: {
            tag: 'div',
            defaultStyles: {
                backgroundColor: '#1c2128',
                borderRadius: '12px',
                overflow: 'hidden',
                minHeight: '300px',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center'
            },
            defaultContent: `
                <div style="text-align: center; color: #8b949e;">
                    <svg width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" style="margin-bottom: 12px;">
                        <polygon points="1 6 1 22 8 18 16 22 23 18 23 2 16 6 8 2 1 6"></polygon>
                        <line x1="8" y1="2" x2="8" y2="18"></line>
                        <line x1="16" y1="6" x2="16" y2="22"></line>
                    </svg>
                    <p>√÷› Œ—Ìÿ… Google Maps</p>
                </div>
            `,
            canHaveChildren: false,
            category: 'media'
        },
        embed: {
            tag: 'div',
            defaultStyles: {
                backgroundColor: '#1c2128',
                borderRadius: '12px',
                overflow: 'hidden',
                minHeight: '200px',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center'
            },
            defaultContent: `
                <div style="text-align: center; color: #8b949e;">
                    <svg width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" style="margin-bottom: 12px;">
                        <polyline points="16 18 22 12 16 6"></polyline>
                        <polyline points="8 6 2 12 8 18"></polyline>
                    </svg>
                    <p>√÷› „Õ ÊÏ „÷„¯‰ (iframe)</p>
                </div>
            `,
            canHaveChildren: false,
            category: 'media'
        }
    };

    // ???????????????????????????????????????????????????????????????????????????
    // Utility Functions
    // ???????????????????????????????????????????????????????????????????????????
    function generateId() {
        return 'el_' + Math.random().toString(36).substr(2, 9);
    }

    function stylesToString(styles) {
        return Object.entries(styles)
            .map(([key, value]) => `${key.replace(/([A-Z])/g, '-$1').toLowerCase()}: ${value}`)
            .join('; ');
    }

    function deepClone(obj) {
        return JSON.parse(JSON.stringify(obj));
    }

    // ???????????????????????????????????????????????????????????????????????????
    // History Management
    // ???????????????????????????????????????????????????????????????????????????
    function saveState() {
        state.history.past.push(deepClone(state.project));
        if (state.history.past.length > CONFIG.maxHistorySteps) {
            state.history.past.shift();
        }
        state.history.future = [];
    }

    function undo() {
        if (state.history.past.length === 0) return;
        state.history.future.push(deepClone(state.project));
        state.project = state.history.past.pop();
        renderCanvas();
        renderLayers();
        updateStatus(' „ «· —«Ã⁄');
    }

    function redo() {
        if (state.history.future.length === 0) return;
        state.history.past.push(deepClone(state.project));
        state.project = state.history.future.pop();
        renderCanvas();
        renderLayers();
        updateStatus(' „ «·≈⁄«œ…');
    }

    // ???????????????????????????????????????????????????????????????????????????
    // Canvas Rendering
    // ???????????????????????????????????????????????????????????????????????????
    function renderCanvas() {
        const canvas = document.getElementById('canvas-content');
        const placeholder = document.getElementById('canvas-placeholder');
        const currentPage = state.project.pages.find(p => p.id === state.project.currentPage);
        
        if (!currentPage || currentPage.elements.length === 0) {
            canvas.innerHTML = '';
            placeholder.style.display = 'block';
            updateElementCount(0);
            return;
        }

        placeholder.style.display = 'none';
        canvas.innerHTML = '';
        
        currentPage.elements.forEach(element => {
            canvas.appendChild(createElementDOM(element));
        });

        updateElementCount(countElements(currentPage.elements));
    }

    function createElementDOM(element) {
        const template = componentTemplates[element.type];
        if (!template) return document.createElement('div');

        const wrapper = document.createElement('div');
        wrapper.className = 'pb-element';
        wrapper.dataset.id = element.id;
        wrapper.dataset.component = element.type;

        if (template.canHaveChildren) {
            wrapper.classList.add('pb-drop-zone');
        }

        const el = document.createElement(template.tag);
        const styles = { ...template.defaultStyles, ...element.styles };
        el.setAttribute('style', stylesToString(styles));
        
        if (template.defaultAttributes) {
            Object.entries(template.defaultAttributes).forEach(([key, value]) => {
                el.setAttribute(key, element.attributes?.[key] || value);
            });
        }

        if (element.content !== undefined) {
            el.innerHTML = element.content;
        } else if (template.defaultContent) {
            el.innerHTML = template.defaultContent;
        }

        wrapper.appendChild(el);

        // Render children
        if (element.children && element.children.length > 0) {
            element.children.forEach(child => {
                el.appendChild(createElementDOM(child));
            });
        }

        // Event listeners
        wrapper.addEventListener('click', (e) => {
            e.stopPropagation();
            selectElement(element.id);
        });

        wrapper.addEventListener('dragover', handleDragOver);
        wrapper.addEventListener('dragleave', handleDragLeave);
        wrapper.addEventListener('drop', (e) => handleDrop(e, element.id));

        return wrapper;
    }

    function countElements(elements) {
        let count = elements.length;
        elements.forEach(el => {
            if (el.children) {
                count += countElements(el.children);
            }
        });
        return count;
    }

    // ???????????????????????????????????????????????????????????????????????????
    // Drag & Drop
    // ???????????????????????????????????????????????????????????????????????????
    function initDragAndDrop() {
        const componentItems = document.querySelectorAll('.pb-component-item');
        const canvas = document.getElementById('canvas');
        const canvasContent = document.getElementById('canvas-content');

        componentItems.forEach(item => {
            item.addEventListener('dragstart', handleDragStart);
            item.addEventListener('dragend', handleDragEnd);
        });

        canvas.addEventListener('dragover', handleDragOver);
        canvas.addEventListener('dragleave', handleDragLeave);
        canvas.addEventListener('drop', handleDrop);

        canvasContent.addEventListener('click', (e) => {
            if (e.target === canvasContent) {
                deselectElement();
            }
        });
    }

    function handleDragStart(e) {
        const componentType = e.target.dataset.component;
        state.ui.isDragging = true;
        state.ui.draggedComponent = componentType;
        e.dataTransfer.setData('text/plain', componentType);
        e.dataTransfer.effectAllowed = 'copy';
        e.target.style.opacity = '0.5';
        document.body.classList.add('dragging');
    }

    function handleDragEnd(e) {
        state.ui.isDragging = false;
        state.ui.draggedComponent = null;
        e.target.style.opacity = '1';
        document.body.classList.remove('dragging');
        
        document.querySelectorAll('.drag-over').forEach(el => {
            el.classList.remove('drag-over');
        });
    }

    function handleDragOver(e) {
        e.preventDefault();
        e.dataTransfer.dropEffect = 'copy';
        
        const target = e.target.closest('.pb-drop-zone, #canvas');
        if (target) {
            target.classList.add('drag-over');
        }
    }

    function handleDragLeave(e) {
        const target = e.target.closest('.pb-drop-zone, #canvas');
        if (target && !target.contains(e.relatedTarget)) {
            target.classList.remove('drag-over');
        }
    }

    function handleDrop(e, parentId = null) {
        e.preventDefault();
        e.stopPropagation();
        
        const componentType = e.dataTransfer.getData('text/plain');
        if (!componentType || !componentTemplates[componentType]) return;

        document.querySelectorAll('.drag-over').forEach(el => {
            el.classList.remove('drag-over');
        });

        saveState();
        addElement(componentType, parentId);
        renderCanvas();
        renderLayers();
        updateStatus(` „ ≈÷«›…: ${componentType}`);
    }

    function addElement(type, parentId = null) {
        const template = componentTemplates[type];
        const newElement = {
            id: generateId(),
            type: type,
            styles: { ...template.defaultStyles },
            content: template.defaultContent || '',
            attributes: template.defaultAttributes ? { ...template.defaultAttributes } : {},
            children: template.canHaveChildren ? [] : undefined
        };

        const currentPage = state.project.pages.find(p => p.id === state.project.currentPage);
        
        if (parentId) {
            const parent = findElement(currentPage.elements, parentId);
            if (parent && parent.children !== undefined) {
                parent.children.push(newElement);
            }
        } else {
            currentPage.elements.push(newElement);
        }

        selectElement(newElement.id);
        return newElement;
    }

    function findElement(elements, id) {
        for (const element of elements) {
            if (element.id === id) return element;
            if (element.children) {
                const found = findElement(element.children, id);
                if (found) return found;
            }
        }
        return null;
    }

    // ???????????????????????????????????????????????????????????????????????????
    // Element Selection & Properties
    // ???????????????????????????????????????????????????????????????????????????
    function selectElement(id) {
        state.ui.selectedElement = id;
        
        document.querySelectorAll('.pb-element').forEach(el => {
            el.classList.remove('selected');
        });

        const element = document.querySelector(`[data-id="${id}"]`);
        if (element) {
            element.classList.add('selected');
        }

        showProperties(id);
    }

    function deselectElement() {
        state.ui.selectedElement = null;
        
        document.querySelectorAll('.pb-element').forEach(el => {
            el.classList.remove('selected');
        });
        
        hideProperties();
    }

    function showProperties(id) {
        const propertiesEmpty = document.getElementById('properties-empty');
        const propertiesContent = document.getElementById('properties-content');
        const elementActions = document.getElementById('element-actions');
        
        const currentPage = state.project.pages.find(p => p.id === state.project.currentPage);
        const element = findElement(currentPage.elements, id);
        
        if (!element) return;

        propertiesEmpty.style.display = 'none';
        propertiesContent.style.display = 'block';
        elementActions.style.display = 'flex';

        // Populate property fields
        populateProperties(element);
    }

    function hideProperties() {
        const propertiesEmpty = document.getElementById('properties-empty');
        const propertiesContent = document.getElementById('properties-content');
        const elementActions = document.getElementById('element-actions');
        
        propertiesEmpty.style.display = 'flex';
        propertiesContent.style.display = 'none';
        elementActions.style.display = 'none';
    }

    function populateProperties(element) {
        // This would populate all the property inputs with the element's current values
        // For brevity, showing just a few examples
        const bgColorInput = document.getElementById('prop-bg-color');
        const bgColorHex = document.getElementById('prop-bg-color-hex');
        
        if (element.styles.backgroundColor) {
            const color = element.styles.backgroundColor;
            if (color.startsWith('#')) {
                bgColorInput.value = color;
                bgColorHex.value = color;
            }
        }
    }

    // ???????????????????????????????????????????????????????????????????????????
    // Element Actions
    // ???????????????????????????????????????????????????????????????????????????
    function deleteElement(id) {
        saveState();
        const currentPage = state.project.pages.find(p => p.id === state.project.currentPage);
        removeElementById(currentPage.elements, id);
        deselectElement();
        renderCanvas();
        renderLayers();
        updateStatus(' „ «·Õ–›');
    }

    function removeElementById(elements, id) {
        const index = elements.findIndex(el => el.id === id);
        if (index !== -1) {
            elements.splice(index, 1);
            return true;
        }
        for (const element of elements) {
            if (element.children && removeElementById(element.children, id)) {
                return true;
            }
        }
        return false;
    }

    function duplicateElement(id) {
        saveState();
        const currentPage = state.project.pages.find(p => p.id === state.project.currentPage);
        const element = findElement(currentPage.elements, id);
        if (!element) return;

        const duplicate = deepClone(element);
        duplicate.id = generateId();
        assignNewIds(duplicate);

        // Find parent and insert after original
        const parent = findParent(currentPage.elements, id);
        const targetArray = parent ? parent.children : currentPage.elements;
        const index = targetArray.findIndex(el => el.id === id);
        targetArray.splice(index + 1, 0, duplicate);

        renderCanvas();
        renderLayers();
        selectElement(duplicate.id);
        updateStatus(' „ «·‰”Œ');
    }

    function assignNewIds(element) {
        element.id = generateId();
        if (element.children) {
            element.children.forEach(child => assignNewIds(child));
        }
    }

    function findParent(elements, childId, parent = null) {
        for (const element of elements) {
            if (element.id === childId) return parent;
            if (element.children) {
                const found = findParent(element.children, childId, element);
                if (found !== undefined) return found;
            }
        }
        return undefined;
    }

    function moveElement(id, direction) {
        saveState();
        const currentPage = state.project.pages.find(p => p.id === state.project.currentPage);
        const parent = findParent(currentPage.elements, id);
        const targetArray = parent ? parent.children : currentPage.elements;
        const index = targetArray.findIndex(el => el.id === id);
        
        if (direction === 'up' && index > 0) {
            [targetArray[index], targetArray[index - 1]] = [targetArray[index - 1], targetArray[index]];
        } else if (direction === 'down' && index < targetArray.length - 1) {
            [targetArray[index], targetArray[index + 1]] = [targetArray[index + 1], targetArray[index]];
        }

        renderCanvas();
        renderLayers();
    }

    // ???????????????????????????????????????????????????????????????????????????
    // Layers Panel
    // ???????????????????????????????????????????????????????????????????????????
    function renderLayers() {
        const layersTree = document.getElementById('layers-tree');
        const currentPage = state.project.pages.find(p => p.id === state.project.currentPage);
        
        if (!currentPage || currentPage.elements.length === 0) {
            layersTree.innerHTML = '<div style="color: var(--pb-text-placeholder); padding: 8px; font-size: 12px;">·«  ÊÃœ ⁄‰«’—</div>';
            return;
        }

        layersTree.innerHTML = '';
        currentPage.elements.forEach(element => {
            layersTree.appendChild(createLayerItem(element, 0));
        });
    }

    function createLayerItem(element, depth) {
        const item = document.createElement('div');
        item.className = 'pb-layer-item';
        item.style.paddingRight = `${12 + depth * 16}px`;
        item.dataset.id = element.id;
        
        if (state.ui.selectedElement === element.id) {
            item.classList.add('selected');
        }

        item.innerHTML = `
            <span style="font-size: 12px; color: var(--pb-text-secondary);">${element.type}</span>
        `;

        item.addEventListener('click', () => selectElement(element.id));

        const container = document.createDocumentFragment();
        container.appendChild(item);

        if (element.children && element.children.length > 0) {
            element.children.forEach(child => {
                container.appendChild(createLayerItem(child, depth + 1));
            });
        }

        return container;
    }

    // ???????????????????????????????????????????????????????????????????????????
    // Code Generation
    // ???????????????????????????????????????????????????????????????????????????
    function generateHTML() {
        const currentPage = state.project.pages.find(p => p.id === state.project.currentPage);
        if (!currentPage) return '';

        let html = '<!DOCTYPE html>\n<html lang="ar" dir="rtl">\n<head>\n';
        html += '    <meta charset="UTF-8">\n';
        html += '    <meta name="viewport" content="width=device-width, initial-scale=1.0">\n';
        html += `    <title>${state.project.name}</title>\n`;
        html += '    <link rel="stylesheet" href="styles.css">\n';
        html += '</head>\n<body>\n';
        
        currentPage.elements.forEach(element => {
            html += generateElementHTML(element, 1);
        });
        
        html += '</body>\n</html>';
        return html;
    }

    function generateElementHTML(element, indent) {
        const template = componentTemplates[element.type];
        const spaces = '    '.repeat(indent);
        const styles = stylesToString({ ...template.defaultStyles, ...element.styles });
        
        let html = `${spaces}<${template.tag}`;
        html += ` style="${styles}"`;
        
        if (element.attributes) {
            Object.entries(element.attributes).forEach(([key, value]) => {
                html += ` ${key}="${value}"`;
            });
        }
        
        if (template.tag === 'img' || template.tag === 'hr') {
            html += ' />\n';
        } else {
            html += '>\n';
            
            if (element.content) {
                html += `${spaces}    ${element.content}\n`;
            }
            
            if (element.children && element.children.length > 0) {
                element.children.forEach(child => {
                    html += generateElementHTML(child, indent + 1);
                });
            }
            
            html += `${spaces}</${template.tag}>\n`;
        }
        
        return html;
    }

    function generateCSS() {
        let css = `/* ${state.project.name} - Generated CSS */\n\n`;
        css += `* {\n    box-sizing: border-box;\n    margin: 0;\n    padding: 0;\n}\n\n`;
        css += `body {\n    font-family: 'Inter', sans-serif;\n    background: #0a0e17;\n    color: #c9d1d9;\n    line-height: 1.6;\n}\n`;
        return css;
    }

    function generateRazor() {
        const currentPage = state.project.pages.find(p => p.id === state.project.currentPage);
        if (!currentPage) return '';

        let razor = `@{\n    ViewData["Title"] = "${state.project.name}";\n}\n\n`;
        
        currentPage.elements.forEach(element => {
            razor += generateElementHTML(element, 0);
        });
        
        return razor;
    }

    // ???????????????????????????????????????????????????????????????????????????
    // View & Zoom Controls
    // ???????????????????????????????????????????????????????????????????????????
    function setView(view) {
        state.ui.view = view;
        const canvas = document.getElementById('canvas');
        canvas.dataset.view = view;
        
        document.querySelectorAll('.pb-view-btn').forEach(btn => {
            btn.classList.toggle('active', btn.dataset.view === view);
        });
    }

    function setZoom(zoom) {
        state.ui.zoom = Math.max(CONFIG.minZoom, Math.min(CONFIG.maxZoom, zoom));
        const container = document.getElementById('canvas-container');
        container.style.transform = `scale(${state.ui.zoom / 100})`;
        document.getElementById('zoom-value').textContent = `${state.ui.zoom}%`;
    }

    // ???????????????????????????????????????????????????????????????????????????
    // Modals
    // ???????????????????????????????????????????????????????????????????????????
    function showPreview() {
        const modal = document.getElementById('preview-modal');
        const iframe = document.getElementById('preview-frame');
        const html = generateHTML();
        
        modal.classList.add('active');
        iframe.srcdoc = html;
    }

    function showCode() {
        const modal = document.getElementById('code-modal');
        modal.classList.add('active');
        updateCodeView('html');
    }

    function updateCodeView(type) {
        const codeElement = document.getElementById('generated-code');
        
        document.querySelectorAll('.pb-modal__tab').forEach(tab => {
            tab.classList.toggle('active', tab.dataset.code === type);
        });
        
        switch (type) {
            case 'html':
                codeElement.textContent = generateHTML();
                break;
            case 'css':
                codeElement.textContent = generateCSS();
                break;
            case 'razor':
                codeElement.textContent = generateRazor();
                break;
        }
    }

    function closeModal(modalId) {
        document.getElementById(modalId).classList.remove('active');
    }

    // ???????????????????????????????????????????????????????????????????????????
    // Save & Publish
    // ???????????????????????????????????????????????????????????????????????????
    async function saveProject() {
        try {
            const response = await fetch('/api/builder/platform/save', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value || ''
                },
                body: JSON.stringify(state.project)
            });
            
            if (response.ok) {
                updateStatus(' „ «·Õ›Ÿ »‰Ã«Õ');
            } else {
                updateStatus('›‘· «·Õ›Ÿ', 'error');
            }
        } catch (error) {
            console.error('Save error:', error);
            // Fallback to localStorage
            localStorage.setItem('hrce_platform_project', JSON.stringify(state.project));
            updateStatus(' „ «·Õ›Ÿ „Õ·Ì«');
        }
    }

    async function publishProject() {
        try {
            const response = await fetch('/api/builder/platform/publish', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    project: state.project,
                    html: generateHTML(),
                    css: generateCSS()
                })
            });
            
            if (response.ok) {
                const result = await response.json();
                updateStatus(` „ «·‰‘—: ${result.url}`);
            } else {
                updateStatus('›‘· «·‰‘—', 'error');
            }
        } catch (error) {
            console.error('Publish error:', error);
            updateStatus('Œÿ√ ›Ì «·‰‘—', 'error');
        }
    }

    function loadProject() {
        const saved = localStorage.getItem('hrce_platform_project');
        if (saved) {
            try {
                state.project = JSON.parse(saved);
                renderCanvas();
                renderLayers();
                document.getElementById('project-name').value = state.project.name;
            } catch (e) {
                console.error('Failed to load project:', e);
            }
        }
    }

    // ???????????????????????????????????????????????????????????????????????????
    // Status Updates
    // ???????????????????????????????????????????????????????????????????????????
    function updateStatus(message, type = 'info') {
        const statusElement = document.getElementById('status-message');
        statusElement.textContent = message;
        statusElement.style.color = type === 'error' ? 'var(--pb-danger)' : 'var(--pb-text-tertiary)';
    }

    function updateElementCount(count) {
        document.getElementById('element-count').textContent = `${count} ⁄‰«’—`;
    }

    // ???????????????????????????????????????????????????????????????????????????
    // Keyboard Shortcuts
    // ???????????????????????????????????????????????????????????????????????????
    function initKeyboardShortcuts() {
        document.addEventListener('keydown', (e) => {
            const isMeta = e.ctrlKey || e.metaKey;
            
            if (isMeta && e.key === 'z') {
                e.preventDefault();
                if (e.shiftKey) {
                    redo();
                } else {
                    undo();
                }
            }
            
            if (isMeta && e.key === 's') {
                e.preventDefault();
                saveProject();
            }
            
            if (e.key === 'Delete' || e.key === 'Backspace') {
                if (state.ui.selectedElement && document.activeElement.tagName !== 'INPUT') {
                    e.preventDefault();
                    deleteElement(state.ui.selectedElement);
                }
            }
            
            if (isMeta && e.key === 'd') {
                if (state.ui.selectedElement) {
                    e.preventDefault();
                    duplicateElement(state.ui.selectedElement);
                }
            }
            
            if (e.key === 'Escape') {
                deselectElement();
                closeModal('preview-modal');
                closeModal('code-modal');
            }
        });
    }

    // ???????????????????????????????????????????????????????????????????????????
    // Property Panel Tabs
    // ???????????????????????????????????????????????????????????????????????????
    function initPropertyTabs() {
        document.querySelectorAll('.pb-sidebar__tab').forEach(tab => {
            tab.addEventListener('click', () => {
                const tabName = tab.dataset.tab;
                
                document.querySelectorAll('.pb-sidebar__tab').forEach(t => {
                    t.classList.toggle('active', t.dataset.tab === tabName);
                });
                
                document.querySelectorAll('.pb-tab-content').forEach(content => {
                    content.classList.toggle('active', content.dataset.tab === tabName);
                });
            });
        });
    }

    // ???????????????????????????????????????????????????????????????????????????
    // Initialize
    // ???????????????????????????????????????????????????????????????????????????
    function init() {
        initDragAndDrop();
        initKeyboardShortcuts();
        initPropertyTabs();
        loadProject();
        renderCanvas();
        renderLayers();

        // Event Listeners
        document.getElementById('btn-undo').addEventListener('click', undo);
        document.getElementById('btn-redo').addEventListener('click', redo);
        document.getElementById('btn-preview').addEventListener('click', showPreview);
        document.getElementById('btn-code').addEventListener('click', showCode);
        document.getElementById('btn-save').addEventListener('click', saveProject);
        document.getElementById('btn-publish').addEventListener('click', publishProject);
        
        document.getElementById('close-preview').addEventListener('click', () => closeModal('preview-modal'));
        document.getElementById('close-code').addEventListener('click', () => closeModal('code-modal'));
        
        document.querySelectorAll('.pb-modal__backdrop').forEach(backdrop => {
            backdrop.addEventListener('click', () => {
                closeModal('preview-modal');
                closeModal('code-modal');
            });
        });

        document.querySelectorAll('.pb-modal__tab').forEach(tab => {
            tab.addEventListener('click', () => updateCodeView(tab.dataset.code));
        });

        document.getElementById('btn-copy-code').addEventListener('click', () => {
            const code = document.getElementById('generated-code').textContent;
            navigator.clipboard.writeText(code).then(() => {
                updateStatus(' „ ‰”Œ «·ﬂÊœ');
            });
        });

        // View controls
        document.querySelectorAll('.pb-view-btn').forEach(btn => {
            btn.addEventListener('click', () => setView(btn.dataset.view));
        });

        document.getElementById('zoom-in').addEventListener('click', () => setZoom(state.ui.zoom + CONFIG.zoomStep));
        document.getElementById('zoom-out').addEventListener('click', () => setZoom(state.ui.zoom - CONFIG.zoomStep));

        // Element actions
        document.getElementById('btn-delete').addEventListener('click', () => {
            if (state.ui.selectedElement) deleteElement(state.ui.selectedElement);
        });
        document.getElementById('btn-duplicate').addEventListener('click', () => {
            if (state.ui.selectedElement) duplicateElement(state.ui.selectedElement);
        });
        document.getElementById('btn-move-up').addEventListener('click', () => {
            if (state.ui.selectedElement) moveElement(state.ui.selectedElement, 'up');
        });
        document.getElementById('btn-move-down').addEventListener('click', () => {
            if (state.ui.selectedElement) moveElement(state.ui.selectedElement, 'down');
        });

        // Project name
        document.getElementById('project-name').addEventListener('change', (e) => {
            state.project.name = e.target.value;
        });

        // Component search
        document.getElementById('component-search').addEventListener('input', (e) => {
            const query = e.target.value.toLowerCase();
            document.querySelectorAll('.pb-component-item').forEach(item => {
                const text = item.textContent.toLowerCase();
                item.style.display = text.includes(query) ? '' : 'none';
            });
        });

        // Auto-save
        setInterval(() => {
            localStorage.setItem('hrce_platform_project', JSON.stringify(state.project));
        }, CONFIG.autoSaveInterval);

        updateStatus('Ã«Â“ ··»‰«¡');
    }

    // Start
    document.addEventListener('DOMContentLoaded', init);
})();
