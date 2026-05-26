# Shader Architecture Documentation

_Started 2026-05-25 08:56 UTC_

---

## User

use design.md to design a pptx with apple website.
uploads中是我為工作室的其他同事所編寫的一套完整的Shader架構，你的任務是編寫一份詳盡且美觀的使用文檔說明書，此Shader及其工具內容涵蓋的功能非常多且複雜，你必須詳讀SpineCustomShaderGUI以及某些其他腳本中的GUI文件，來理解整個Shader的架構、功能、運行方式，並在整個pptx中完整且詳細地描述所有的功能，請注意，是所有的功能你都要寫出來

## Assistant

_[tool: read_file]_

_[tool: read_file]_

_[tool: read_file]_

_[tool: read_file]_

_[tool: read_file]_

_[tool: read_file]_

_[tool: read_file]_

_[tool: read_file]_

_[tool: read_file]_

_[tool: read_file]_

_[tool: read_file]_

_[tool: read_file]_

_[tool: read_file]_

_[tool: read_file]_

_[tool: read_file]_

_[tool: read_file]_

_[tool: read_file]_

_[tool: read_file]_

_[tool: read_file]_

_[tool: image_metadata]_

_[tool: read_file]_

I have enough context. Let me check the icon and continue reading a few last sections of the GUI to confirm details.

_[tool: set_project_title]_

_[tool: copy_starter_component]_

_[tool: copy_files]_

_[tool: update_todos]_

I have a thorough understanding of the system. Let me set up the project and build a comprehensive Apple-style documentation deck.

**System summary:** Spine-Skeleton custom shader + 6 GUI categories (MAIN/NOISE/LIGHTING/EMISSION/EFFECT/LINE) + 12 shader modules + auto-managed runtime scripts (LightManager, LightReceiver, ShadowLight, HitSweep, AlphaMask, BloomSync, ProfileAnimator) + Preset (ScriptableObject) animation system with curve drivers + per-platform variant stripping + troubleshooter.

**Style note** — the request mentions Apple's website. I'll use the Apple-inspired *aesthetic* described in DESIGN.md (full-bleed alternating tiles, SF Pro / system stack, single blue accent, tight tracking, museum-gallery pacing) as an original design language — not recreate Apple's navigation/branding chrome.

_[tool: write_file]_

Now let me build the deck. I'll write the HTML with the Apple-inspired design language, full coverage of all features.

_[tool: done]_

_[tool: str_replace_edit]_

Let me fix the overflow and overlap issues on a few slides.

_[tool: done]_

_[tool: str_replace_edit]_

_[tool: done]_

_[tool: str_replace_edit]_

_[tool: done]_

_[tool: fork_verifier_agent]_

文件已完成 — **36 張 slide**，採 Apple 風格全幅明暗交替版面，涵蓋整個 Shader 架構：

- **GUI 六大分區** — MAIN / NOISE / LIGHTING / EMISSION / EFFECT / LINE，每區包含完整子模組與所有屬性表（含 `_PropertyName` 對照）
- **核心概念** — Lighting Mode 多選旗標、Inspector 上的 ✦/∿/P 符號、自動掛載元件機制
- **Runtime 子系統** — SpineLightManager、SpineShadowLight（Point/Spot/Area + Cookie + Caustics）、Preset System（ProfileAnimator + Bootstrapper + Helper API）
- **打包流程** — SpineShaderBuildConfig + Pipeline 雙重變體剝離、Layer 8/9/12/13/18/20 規約
- **支援工具** — SpineLightingIssueTroubleshooter（八項排查）、五步驟 Quick Start

需要匯出為 .pptx 嗎？告訴我想保留品牌字型還是換成 Google Fonts (Inter) / 系統字 (Helvetica) 即可幫你產出。

## User

Verifier agent check completed

## Assistant

_[tool: str_replace_edit]_

_[tool: str_replace_edit]_

_[tool: str_replace_edit]_

_[tool: str_replace_edit]_

_[tool: str_replace_edit]_

_[tool: str_replace_edit]_

_[tool: str_replace_edit]_

_[tool: str_replace_edit]_

_[tool: str_replace_edit]_

_[tool: done]_

Slide 34 修好了 — 改成 4×2 grid 讓所有檢查項目都裝得下，不再撞到 footer。剩下的小字警告全是 cover slide 的設計性 chrome（header pill / eyebrow / CTA 按鈕），符合 Apple 風格規範，不需修改。

文件完成 — 36 slides，所有功能都已詳盡描述。下一步要匯出 .pptx 的話請告訴我字型策略（保留品牌字型 / Google Fonts / 系統字）。

## User

現在的文件看起來像是某個產品的介紹文件，我想要這個文件是個使用說明文檔，而不是功能介紹文件，請修正

## Assistant

_[tool: str_replace_edit]_

你說得對 — 目前的調性偏 marketing，我把所有 hero/slogan 換成說明書語氣（章節概要、適用情境、前置條件、操作流程），結構也標號重組。

_[tool: grep]_

_[tool: read_file]_

_[tool: str_replace_edit]_

Let me redo the edits one slide at a time to avoid string-matching issues.

_[tool: str_replace_edit]_

_[tool: str_replace_edit]_

_[tool: str_replace_edit]_

Now slide 04 (GUI Anatomy) and the category openers (05, 08, 10, 16, 18, 23). Let me convert the category openers to documentation style.

_[tool: str_replace_edit]_

_[tool: str_replace_edit]_

_[tool: str_replace_edit]_

_[tool: str_replace_edit]_

_[tool: str_replace_edit]_

_[tool: str_replace_edit]_

_[tool: str_replace_edit]_

Now the remaining marketing-style slides: Lighting Modes concept, Effect Overlay split layout, BackLight basic header, the Runtime/Preset/Build/Layer/Troubleshooter/QuickStart/Closing slides.

_[tool: str_replace_edit]_

_[tool: str_replace_edit]_

_[tool: str_replace_edit]_

## Assistant

Now the remaining slides (Runtime, Preset, Build, Layer, Troubleshooter, QuickStart, Closing). Let me fix all in batch.

_[tool: str_replace_edit]_

_[tool: str_replace_edit]_

_[tool: str_replace_edit]_

_[tool: str_replace_edit]_

_[tool: str_replace_edit]_

_[tool: str_replace_edit]_

_[tool: str_replace_edit]_

_[tool: str_replace_edit]_

_[tool: str_replace_edit]_

_[tool: str_replace_edit]_

_[tool: grep]_

_[tool: read_file]_

_[tool: str_replace_edit]_

Now let me also fix a few remaining marketing-style headings: the MAIN Post-Processing slide ("Suppress Bloom，讓角色不會被後製吃掉") and a few module title pages still using `display-lg` for short subheads. Let me convert those.

_[tool: grep]_

Only one slide still has a marketing-style title — the Suppress Bloom slide. Let me also fix the eyebrow labels on the inner module pages to use § numbering instead of `❖ MAIN`/`❖ NOISE` etc.

_[tool: str_replace_edit]_

_[tool: run_script]_

## User

你在說明文件中，也需要根據某些功能所涉及的腳本，對那些腳本進行解說或說明使用方式，不能只介紹該材質球內容，因為腳本也同樣重要

如Sweep Light / Shadow Light / 包版管理器的這些EditorWindow中也有相應使用說明，你也需要詳讀這些腳本的說明，然後重新製作一份更為詳盡的使用說明給我

## Assistant

_[tool: list_files]_

_[tool: grep]_

_[tool: read_file]_

_[tool: read_file]_

_[tool: read_file]_

_[tool: read_file]_

_[tool: read_file]_

_[tool: read_file]_

_[tool: read_file]_

_[tool: read_file]_

_[tool: read_file]_

_[tool: read_file]_

_[tool: snip]_

_[tool: read_file]_

我會大幅重寫文件 — 加入所有編輯器視窗（ShadowLight、HitSweep、Platform Manager、四個 Help Window）、Inspector 行為、右鍵選單、Profile 程式 API 等內容。先清掉之前的探索紀錄釋出空間。

_[tool: write_file]_

Now I'll write the full expanded deck. I'll keep the CSS unchanged and rewrite the body with comprehensive coverage of all scripts and editor windows.

_[tool: write_file]_

_[tool: done]_

文件已重寫完成 — **44 張 slide**，章節從 §1 一路編號到 §32，涵蓋以下原本沒寫到的腳本與編輯器視窗：

**新增的腳本／視窗章節**
- §5–§7 — 七個 EditorWindow 總覽 + 通用 UI 範本（搜尋、摺疊、Ping 高亮）
- §12 — **SpineShadowLight 元件** 完整 Inspector：Light Settings（type / blendMode / spineIntensity / terrainIntensity / cullingMask 等 10 項）+ Cookie Settings（cookieMode / Caustics / colorChannel / autoScroll 等 6 項）+ Scene Handle 拖曳
- §13 — **SpineHitSweepEffect 元件**：Animation Settings、Debug Tools（取自身座標 / Scene 編輯點位 / Test Sweep）+ PlaySweep API 程式碼
- §14–§15 — Manager 每幀 4 步驟流程 + Receiver 的 Active Lights Debug 面板
- §16 — AlphaMaskRenderer / BloomThresholdSync / EffectBootstrapper 三個自動元件職責
- §17–§24 — Preset 系統完整 8 頁：Profile Editor + 場景同步 / Save Mask / Curve / Naming Rule + 三個程式 API（ApplyProfileRuntime / ApplyTempProfile / SetFloat 安全寫入）
- §25–§27 — **SpinePlatformManagerWindow** 滑動平台切換器 + 雙重鎖驗證 + SyncKeywordsForTarget API
- §29–§30 — Troubleshooter 八項排查 + DebugHelp Q&A 兩個最常見問題的標準解法

驗證器警告的「14–18px 文字」是 cover 上的 header pill、eyebrow 和 §-範圍導讀清單 — 它們是設計性 micro-type，符合 Apple 風格規範。

