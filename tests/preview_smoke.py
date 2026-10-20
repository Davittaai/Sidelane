"""Optional Chromium QA for the HTML design concept, not WPF runtime QA.

Install: pip install playwright; playwright install chromium
Run: python tests/preview_smoke.py
"""
from pathlib import Path
import shutil
from playwright.sync_api import sync_playwright

root=Path(__file__).resolve().parents[1]
with sync_playwright() as p:
    options={'headless':True}
    if shutil.which('chromium'):
        options['executable_path']=shutil.which('chromium')
        options['args']=['--no-sandbox']
    browser=p.chromium.launch(**options)
    page=browser.new_page(viewport={'width':1440,'height':900})
    errors=[]
    page.on('pageerror',lambda err: errors.append(str(err)))
    page.set_content((root/'preview/index.html').read_text(encoding='utf-8'),wait_until='load')
    page.locator('#openSettings').click()
    assert 'show' in page.locator('#settings').get_attribute('class')
    page.locator('#accent').fill('#ffb86c')
    assert page.locator('#edge').evaluate('(e) => getComputedStyle(e).getPropertyValue("--accent").trim()').lower() == '#ffb86c'
    page.locator('#edgeChoice').select_option('Right')
    assert 'right' in page.locator('#edge').get_attribute('class')
    page.locator('#saveSettings').click()
    assert 'show' not in page.locator('#settings').get_attribute('class')
    page.locator('#taskInput').fill('QA task one')
    page.locator('#addForm button').click()
    assert page.locator('#tasks .task').count() == 3
    page.locator('#tasks .task').last.locator('input').check()
    assert 'done' in page.locator('#tasks .task').last.get_attribute('class')
    page.locator('#tasks .task').last.locator('button.remove').click()
    assert page.locator('#tasks .task').count() == 2
    page.locator('#appFilter').fill('Claude')
    assert page.locator('.apps .app:visible').count() == 1
    page.locator('#appFilter').fill('')
    assert page.locator('.apps .app:visible').count() == 5
    # The task input must be visible without scrolling on the reference desktop viewport.
    assert page.locator('#taskInput').is_visible()
    assert page.locator('#taskInput').bounding_box()['y'] + page.locator('#taskInput').bounding_box()['height'] < page.locator('#edge').bounding_box()['y'] + page.locator('#edge').bounding_box()['height'] - 41
    # Context menu states are user-entered labels, not automatic AI progress.
    page.locator('.app[data-app="Claude"]').click(button='right')
    assert page.locator('#statusMenu').is_visible()
    page.locator('#statusMenu button[data-status="Waiting · manual"]').click()
    assert page.locator('.app[data-app="Claude"] .app-detail').inner_text() == 'Waiting · manual'
    page.keyboard.press('Escape')
    page.keyboard.press('/')
    assert page.locator('#appFilter').evaluate('(el) => el === document.activeElement')
    page.locator('#appFilter').blur()
    previous=page.locator('.song').inner_text()
    page.locator('#next').click()
    assert page.locator('.song').inner_text()!=previous
    page.locator('#closeSidebar').click()
    assert 'closed' in page.locator('#edge').get_attribute('class')
    page.locator('#edge').hover()
    page.wait_for_timeout(270)
    assert 'closed' not in page.locator('#edge').get_attribute('class')
    assert not errors, errors
    browser.close()
print('PASS: design-preview interaction smoke test; zero uncaught page errors')
