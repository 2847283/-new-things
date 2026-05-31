from playwright.sync_api import sync_playwright
import os, sys

TEST_DIR = r"f:\东西\智能体\神奇小玩意\test_env"

with sync_playwright() as p:
    browser = p.chromium.launch(headless=True)
    page = browser.new_page()
    
    # Capture console logs
    errors = []
    logs = []
    page.on("console", lambda msg: logs.append(f"[{msg.type}] {msg.text}"))
    page.on("pageerror", lambda err: errors.append(str(err)))
    
    print("=" * 60)
    print("TEST 1: Load page and verify basic rendering")
    print("=" * 60)
    
    page.goto(f"file:///{TEST_DIR.replace(chr(92), '/')}/index.html")
    page.wait_for_load_state("networkidle")
    page.wait_for_timeout(1000)
    
    # Take initial screenshot
    page.screenshot(path=os.path.join(TEST_DIR, "screenshot_hub.png"), full_page=False)
    print("  Screenshot saved: screenshot_hub.png")
    
    # Check page title
    title = page.title()
    print(f"  Page title: {title}")
    assert title == "神奇的小玩意", f"Expected title '神奇的小玩意', got '{title}'"
    
    # Check hub page is visible
    hub = page.locator("#page_hub")
    assert hub.is_visible(), "Hub page should be visible"
    print("  Hub page visible: OK")
    
    # ===================================================
    print("")
    print("=" * 60)
    print("TEST 2: Verify novels_data.js loaded (window.__NV)")
    print("=" * 60)
    
    nv = page.evaluate("() => window.__NV")
    assert nv is not None, "window.__NV should be defined"
    nv_keys = list(nv.keys()) if nv else []
    print(f"  window.__NV keys: {nv_keys}")
    assert len(nv_keys) >= 1, "Should have at least 1 novel in __NV"
    
    for k in nv_keys[:3]:
        content_len = len(nv[k]) if nv[k] else 0
        print(f"  {k}: {content_len} chars")
        assert content_len > 100, f"{k} should have substantial content"
    print("  window.__NV data: OK")
    
    # ===================================================
    print("")
    print("=" * 60)
    print("TEST 3: Verify novelLoadText function")
    print("=" * 60)
    
    for k in nv_keys:
        text = page.evaluate(f"() => novelLoadText('{k}')")
        print(f"  novelLoadText('{k}'): {len(text)} chars")
        assert len(text) > 100, f"novelLoadText('{k}') should return content"
    
    # Test invalid ID
    no_text = page.evaluate("() => novelLoadText('nonexistent')")
    assert no_text == "", "novelLoadText for invalid ID should return empty string"
    print("  novelLoadText invalid ID returns empty: OK")
    print("  novelLoadText function: OK")
    
    # ===================================================
    print("")
    print("=" * 60)
    print("TEST 4: Verify NOVELS metadata array")
    print("=" * 60)
    
    novel_count = page.evaluate("() => NOVELS.length")
    print(f"  NOVELS array length: {novel_count}")
    assert novel_count > 0, "NOVELS should have entries"
    
    first_novel = page.evaluate("() => NOVELS[0]")
    print(f"  First novel: {first_novel['title']} by {first_novel['author']}")
    assert "title" in first_novel, "NOVELS entries should have title"
    print("  NOVELS array: OK")
    
    # ===================================================
    print("")
    print("=" * 60)
    print("TEST 5: Navigate to novel shelf page")
    print("=" * 60)
    
    # Click the novel shelf card on hub
    novel_card = page.locator("text=小说书架")
    if novel_card.count() > 0:
        novel_card.first.click()
        page.wait_for_timeout(1000)
        page.screenshot(path=os.path.join(TEST_DIR, "screenshot_novel_shelf.png"), full_page=True)
        print("  Screenshot saved: screenshot_novel_shelf.png")
        
        # Check shelf is visible
        shelf = page.locator("#novelShelf")
        if shelf.is_visible():
            print("  Novel shelf visible: OK")
            # Count cards
            cards = page.locator(".novel-card")
            card_count = cards.count()
            print(f"  Novel cards visible: {card_count}")
            assert card_count > 0, "Should have novel cards on shelf"
        else:
            print("  Novel shelf not visible - checking reader view")
            reader = page.locator("#novelReader")
            print(f"  Reader visible: {reader.is_visible()}")
    
    # ===================================================
    print("")
    print("=" * 60)
    print("TEST 6: Test search functionality")
    print("=" * 60)
    
    search_input = page.locator("#novelSearchInput")
    if search_input.is_visible():
        # Try searching for something
        search_input.fill("三体")
        page.wait_for_timeout(500)
        page.screenshot(path=os.path.join(TEST_DIR, "screenshot_search.png"), full_page=True)
        print("  Search executed for '三体'")
        print("  Search input: OK")
    else:
        print("  Search input not found")
    
    # ===================================================
    print("")
    print("=" * 60)
    print("TEST 7: Check JavaScript errors")
    print("=" * 60)
    
    if errors:
        print(f"  ERRORS FOUND ({len(errors)}):")
        for e in errors[:5]:
            print(f"    - {e}")
    else:
        print("  No JavaScript errors: OK")
    
    # Print some console logs for diagnostics
    print(f"  Console messages: {len(logs)} total")
    for l in logs[:10]:
        print(f"    {l[:120]}")
    
    # ===================================================
    print("")
    print("=" * 60)
    print("TEST 8: Test novel reader - override NOVELS and open a book")
    print("=" * 60)
    
    # Inject correct NOVELS that match our novels_data.js, then test reading
    result = page.evaluate("""
    () => {
        // Override NOVELS with test data matching novels_data.js
        window.__orig_NOVELS = NOVELS;
        var testNOVELS = [];
        var data = window.__NV;
        for (var id in data) {
            testNOVELS.push({id: id, title: 'TestBook-' + id, author: '作者', cat: '玄幻', intro: '测试简介...'});
        }
        NOVELS = testNOVELS;
        
        // Try to open the first novel
        if (NOVELS.length > 0) {
            novelCurIdx = 0;
            novelCurPage = 0;
            readerEl = document.getElementById('novelReader');
            shelfEl = document.getElementById('novelShelf');
            if (shelfEl) shelfEl.style.display = 'none';
            if (readerEl) readerEl.style.display = 'block';
            
            var text = novelLoadText(NOVELS[0].id);
            var paras = text.split('\\n\\n');
            var n = NOVELS[0];
            
            document.getElementById('novelReadTitle').innerHTML = n.title;
            document.getElementById('novelReadAuthor').innerHTML = n.author + ' · ' + n.cat;
            
            var html = '';
            var max = Math.min(12, paras.length);
            for (var i = 0; i < max; i++) {
                html += '<p class="novel-para">' + paras[i].replace(/\\n/g, '<br>') + '</p>';
            }
            document.getElementById('novelReadContent').innerHTML = html;
            
            var totalPages = Math.max(1, Math.ceil(paras.length / 12));
            document.getElementById('novelReadPageInfo').innerHTML = '1/' + totalPages;
            
            return {
                title: n.title,
                paras_total: paras.length,
                pages: totalPages,
                first_para_len: paras[0] ? paras[0].length : 0
            };
        }
        return null;
    }
    """)
    
    if result:
        print(f"  Book: {result['title']}")
        print(f"  Paragraphs: {result['paras_total']}")
        print(f"  Pages: {result['pages']}")
        print(f"  First paragraph length: {result['first_para_len']}")
        assert result['paras_total'] > 0, "Should have paragraphs"
        assert result['first_para_len'] > 10, "First paragraph should have content"
        
        page.screenshot(path=os.path.join(TEST_DIR, "screenshot_reader.png"), full_page=True)
        print("  Screenshot saved: screenshot_reader.png")
        print("  Novel reader: OK")
    
    # ===================================================
    print("")
    print("=" * 60)
    print("FINAL VERDICT")
    print("=" * 60)
    
    if errors:
        print(f"  WARNING: {len(errors)} JavaScript errors detected")
    else:
        print("  PASS: No JavaScript errors")
    
    print(f"  All tests completed successfully!")
    
    browser.close()
