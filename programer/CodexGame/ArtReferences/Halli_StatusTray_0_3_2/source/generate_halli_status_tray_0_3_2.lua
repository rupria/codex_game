-- Halli Status/Tray 0.3.2
-- Deterministic Aseprite source generator for Unity-ready UI PNGs and layout previews.

local p = app.params
local runtimeDir = assert(p.runtimeDir, "runtimeDir is required")
local sourceDir = assert(p.sourceDir, "sourceDir is required")
local previewDir = assert(p.previewDir, "previewDir is required")
local outputDir = assert(p.outputDir, "outputDir is required")
local screenshotPath = assert(p.screenshot, "screenshot is required")
local cardAPath = assert(p.cardA, "cardA is required")
local cardBPath = assert(p.cardB, "cardB is required")
local cardCPath = assert(p.cardC, "cardC is required")

local C = {
  transparent = Color{ r=0, g=0, b=0, a=0 },
  ink = Color{ r=3, g=7, b=9, a=255 },
  shadow = Color{ r=8, g=7, b=6, a=255 },
  deep = Color{ r=4, g=12, b=16, a=246 },
  panel = Color{ r=5, g=19, b=29, a=248 },
  panel2 = Color{ r=8, g=29, b=39, a=248 },
  muted = Color{ r=29, g=44, b=48, a=255 },
  brassDark = Color{ r=71, g=43, b=14, a=255 },
  brass = Color{ r=151, g=94, b=25, a=255 },
  brassHi = Color{ r=225, g=161, b=48, a=255 },
  cream = Color{ r=238, g=224, b=184, a=255 },
  cyanDark = Color{ r=0, g=91, b=107, a=255 },
  cyan = Color{ r=0, g=226, b=239, a=255 },
  cyanHi = Color{ r=131, g=255, b=248, a=255 },
  redDark = Color{ r=111, g=20, b=29, a=255 },
  red = Color{ r=255, g=55, b=65, a=255 },
  redHi = Color{ r=255, g=144, b=111, a=255 },
  table = Color{ r=5, g=19, b=14, a=255 },
}

local function fill(img, x, y, w, h, color)
  local x0 = math.max(0, math.floor(x))
  local y0 = math.max(0, math.floor(y))
  local x1 = math.min(img.width - 1, math.floor(x + w - 1))
  local y1 = math.min(img.height - 1, math.floor(y + h - 1))
  for yy=y0,y1 do
    for xx=x0,x1 do img:drawPixel(xx, yy, color) end
  end
end

local function stroke(img, x, y, w, h, color, thickness)
  thickness = thickness or 1
  fill(img, x, y, w, thickness, color)
  fill(img, x, y+h-thickness, w, thickness, color)
  fill(img, x, y, thickness, h, color)
  fill(img, x+w-thickness, y, thickness, h, color)
end

local function hline(img, x0, x1, y, color)
  fill(img, x0, y, x1-x0+1, 1, color)
end

local function vline(img, x, y0, y1, color)
  fill(img, x, y0, 1, y1-y0+1, color)
end

local function copyRect(src, dst, sx, sy, w, h, dx, dy)
  for y=0,h-1 do
    for x=0,w-1 do
      local tx, ty = dx+x, dy+y
      local ux, uy = sx+x, sy+y
      if tx >= 0 and ty >= 0 and tx < dst.width and ty < dst.height and
         ux >= 0 and uy >= 0 and ux < src.width and uy < src.height then
        dst:drawPixel(tx, ty, src:getPixel(ux, uy))
      end
    end
  end
end

local function paste(src, dst, dx, dy)
  copyRect(src, dst, 0, 0, src.width, src.height, dx, dy)
end

local function resizeNearest(src, newW, newH)
  local dst = Image(newW, newH, ColorMode.RGB)
  dst:clear(C.transparent)
  for y=0,newH-1 do
    local sy = math.min(src.height-1, math.floor(y * src.height / newH))
    for x=0,newW-1 do
      local sx = math.min(src.width-1, math.floor(x * src.width / newW))
      dst:drawPixel(x, y, src:getPixel(sx, sy))
    end
  end
  return dst
end

local function loadImage(path)
  local spr = app.open(path)
  assert(spr, "Cannot open " .. path)
  local img = Image(spr.cels[1].image)
  spr:close()
  return img
end

local function setSpriteImage(spr, img)
  local cel = spr.cels[1]
  cel.image:clear(C.transparent)
  cel.image:drawImage(img, Point(0, 0))
end

local function savePng(img, path)
  local spr = Sprite(img.width, img.height, ColorMode.RGB)
  setSpriteImage(spr, img)
  spr:saveAs(path)
  spr:close()
end

local heartRows = {
  [2]={{5,9},{14,18}},
  [3]={{3,10},{13,20}},
  [4]={{2,21}}, [5]={{1,22}}, [6]={{1,22}}, [7]={{1,22}},
  [8]={{2,21}}, [9]={{2,21}}, [10]={{3,20}}, [11]={{4,19}},
  [12]={{5,18}}, [13]={{6,17}}, [14]={{7,16}}, [15]={{8,15}},
  [16]={{9,14}}, [17]={{10,13}}, [18]={{11,12}}
}

local function inHeart(x, y)
  local spans = heartRows[y]
  if not spans then return false end
  for _,s in ipairs(spans) do if x >= s[1] and x <= s[2] then return true end end
  return false
end

local function heartImage(team, state)
  local img = Image(24, 24, ColorMode.RGB)
  img:clear(C.transparent)
  local main = team == "player" and C.cyan or C.red
  local hi = team == "player" and C.cyanHi or C.redHi
  local dark = team == "player" and C.cyanDark or C.redDark
  local fillColor = state == "filled" and main or Color{r=8,g=12,b=14,a=255}

  for y=0,23 do
    for x=0,23 do
      local inside = inHeart(x,y)
      local near = inHeart(x-1,y) or inHeart(x+1,y) or inHeart(x,y-1) or inHeart(x,y+1)
      if not inside and near then
        img:drawPixel(x,y,C.ink)
      elseif inside then
        local edge = not inHeart(x-1,y) or not inHeart(x+1,y) or not inHeart(x,y-1) or not inHeart(x,y+1)
        if edge then img:drawPixel(x,y,dark) else img:drawPixel(x,y,fillColor) end
      end
    end
  end

  if state == "filled" then
    fill(img, 4, 5, 5, 2, hi)
    fill(img, 3, 7, 3, 2, hi)
    hline(img, 8, 17, 16, dark)
  else
    local crack = {{12,5},{11,6},{12,7},{10,8},{11,9},{9,10},{10,11},{9,12},{8,13},{9,14}}
    for _,pt in ipairs(crack) do img:drawPixel(pt[1],pt[2], state == "damage" and C.brassHi or main) end
    if state == "damage" then
      fill(img, 3, 5, 7, 4, main)
      fill(img, 4, 9, 5, 2, dark)
    end
  end
  return img
end

local function drawDiamond(img, cx, cy, radius, color)
  for yy=-radius,radius do
    local span = radius - math.abs(yy)
    hline(img, cx-span, cx+span, cy+yy, color)
  end
end

local function drawStackGlyph(img, x, y, accent)
  fill(img, x+5, y, 26, 36, C.ink)
  stroke(img, x+5, y, 26, 36, C.brassDark, 2)
  fill(img, x+2, y+4, 26, 36, C.ink)
  stroke(img, x+2, y+4, 26, 36, C.brass, 2)
  fill(img, x, y+8, 26, 36, C.panel2)
  stroke(img, x, y+8, 26, 36, accent, 2)
  drawDiamond(img, x+13, y+26, 4, C.brassHi)
end

local function playerTrayBase()
  local img = Image(378, 130, ColorMode.RGB)
  img:clear(C.transparent)
  fill(img, 5, 11, 368, 116, C.shadow)
  stroke(img, 4, 9, 370, 118, C.ink, 3)
  stroke(img, 7, 12, 364, 112, C.brassDark, 2)
  hline(img, 12, 365, 16, C.brassHi)
  hline(img, 12, 365, 119, C.brass)
  fill(img, 11, 19, 356, 98, C.deep)
  stroke(img, 11, 19, 356, 98, C.muted, 1)

  -- Unambiguous summary well: card-stack glyph, never a plus sign.
  fill(img, 17, 29, 72, 78, C.panel)
  stroke(img, 17, 29, 72, 78, C.cyanDark, 2)
  hline(img, 22, 84, 102, C.cyan, 2)
  drawStackGlyph(img, 39, 44, C.cyan)

  -- One continuous dynamic viewport. No baked empty card slots.
  fill(img, 101, 27, 258, 82, C.panel)
  stroke(img, 101, 27, 258, 82, C.cyanDark, 1)
  hline(img, 105, 355, 105, C.cyan, 2)
  vline(img, 96, 25, 111, C.brassDark)
  vline(img, 97, 25, 111, C.brassHi)

  drawDiamond(img, 189, 12, 5, C.ink)
  drawDiamond(img, 189, 12, 3, C.cyan)
  drawDiamond(img, 189, 12, 1, C.cyanHi)
  return img
end

local function aiStatusBase()
  local img = Image(224, 130, ColorMode.RGB)
  img:clear(C.transparent)
  fill(img, 5, 11, 214, 116, C.shadow)
  stroke(img, 4, 9, 216, 118, C.ink, 3)
  stroke(img, 7, 12, 210, 112, C.brassDark, 2)
  hline(img, 12, 211, 16, C.brassHi)
  fill(img, 11, 19, 202, 98, C.deep)
  stroke(img, 11, 19, 202, 98, C.redDark, 1)
  hline(img, 16, 208, 105, C.red, 2)

  -- AI lower-right is status-only: no duplicate deck/card-back art.
  fill(img, 20, 31, 78, 74, C.panel)
  stroke(img, 20, 31, 78, 74, C.redDark, 1)
  local dots={{44,45},{55,45},{63,53},{63,64},{55,72},{44,72},{36,64},{36,53}}
  for _,pt in ipairs(dots) do fill(img,pt[1],pt[2],4,4,C.cream) end
  fill(img, 112, 47, 20, 34, C.panel2)
  fill(img, 145, 47, 20, 34, C.panel2)
  fill(img, 178, 47, 20, 34, C.panel2)
  stroke(img, 112, 47, 20, 34, C.redDark, 2)
  stroke(img, 145, 47, 20, 34, C.redDark, 2)
  stroke(img, 178, 47, 20, 34, C.redDark, 2)
  return img
end

local function trayState(base, cards, count)
  local img = Image(base)
  -- Left-aligned dynamic list. 52 px step exposes rank/suit of every card.
  for i=1,count do
    paste(cards[((i-1) % #cards)+1], img, 112 + (i-1)*52, 29)
  end
  return img
end

local function makeTraySource(base, cards, path)
  local spr = Sprite(378, 130, ColorMode.RGB)
  spr.layers[1].name = "frame_and_open_viewport"
  setSpriteImage(spr, trayState(base, cards, 0))
  for n=1,3 do
    local fr = spr:newEmptyFrame()
    spr:newCel(spr.layers[1], fr, trayState(base, cards, n), Point(0,0))
  end
  spr:saveAs(path)
  spr:close()
end

local function makeHeartSource(path)
  local names = {
    {"player","filled"},{"player","empty"},{"player","damage"},
    {"ai","filled"},{"ai","empty"},{"ai","damage"}
  }
  local spr = Sprite(24, 24, ColorMode.RGB)
  spr.layers[1].name = "heart_states_player_then_ai"
  setSpriteImage(spr, heartImage(names[1][1],names[1][2]))
  for i=2,#names do
    local fr = spr:newEmptyFrame()
    spr:newCel(spr.layers[1], fr, heartImage(names[i][1],names[i][2]), Point(0,0))
  end
  spr:saveAs(path)
  spr:close()
end

local function pairPreview(cardA, cardB)
  local img = Image(400, 150, ColorMode.RGB)
  img:clear(C.transparent)
  fill(img, 0, 0, 400, 150, C.deep)
  stroke(img, 0, 0, 400, 150, C.brassDark, 2)
  -- Current: 28 px step on 96 px cards, previous card becomes unreadable.
  local bigA = resizeNearest(cardA, 96, 135)
  local bigB = resizeNearest(cardB, 96, 135)
  paste(bigA, img, 12, 8)
  paste(bigB, img, 40, 8)
  -- Approved: 84 px step, both rank/suit and skull count remain visible.
  paste(bigA, img, 205, 8)
  paste(bigB, img, 289, 8)
  vline(img, 182, 8, 141, C.brassHi)
  return img
end

local function applicationPreview(screen, baseTray, aiPanel, hearts, cards)
  local after = Image(screen)

  -- Remove the clipped stale card renderer at the upper-left edge.
  -- Reuse the adjacent wall pixels so the result does not leave a flat patch.
  copyRect(screen, after, 54, 0, 54, 58, 0, 0)

  -- Replace flat health pips with readable western pixel hearts.
  fill(after, 108, 35, 84, 27, Color{r=2,g=7,b=6,a=255})
  fill(after, 620, 35, 84, 27, Color{r=2,g=7,b=6,a=255})
  for i=0,2 do paste(resizeNearest(hearts[i+1],20,20), after, 114+i*25, 39) end
  for i=0,2 do paste(resizeNearest(hearts[i+4],20,20), after, 630+i*25, 39) end

  -- Wider exposed pair: latest card still leads, previous card remains calculable.
  copyRect(screen, after, 500, 148, 190, 120, 190, 148)
  paste(cards[2], after, 216, 164)
  paste(cards[3], after, 280, 164)

  -- Player acquired cards: open viewport, left-aligned actual list.
  fill(after, 24, 307, 330, 111, C.table)
  local smallTray = resizeNearest(baseTray, 302, 104)
  paste(smallTray, after, 28, 314)
  paste(resizeNearest(cards[1],45,62), after, 111, 338)
  paste(resizeNearest(cards[2],45,62), after, 153, 338)

  -- AI lower-right status only; duplicate card/deck illustration removed.
  fill(after, 575, 307, 193, 111, C.table)
  paste(resizeNearest(aiPanel,179,104), after, 585, 314)
  return after
end

local cardA = loadImage(cardAPath)
local cardB = loadImage(cardBPath)
local cardC = loadImage(cardCPath)
local cards = {cardA, cardB, cardC}
local screen = loadImage(screenshotPath)
local baseTray = playerTrayBase()
local aiPanel = aiStatusBase()
local hearts = {
  heartImage("player","filled"), heartImage("player","filled"), heartImage("player","empty"),
  heartImage("ai","filled"), heartImage("ai","filled"), heartImage("ai","empty")
}

local runtime = {
  {"hp_heart_player_filled_24_0_3_2.png", heartImage("player","filled")},
  {"hp_heart_player_empty_24_0_3_2.png", heartImage("player","empty")},
  {"hp_heart_player_damage_24_0_3_2.png", heartImage("player","damage")},
  {"hp_heart_ai_filled_24_0_3_2.png", heartImage("ai","filled")},
  {"hp_heart_ai_empty_24_0_3_2.png", heartImage("ai","empty")},
  {"hp_heart_ai_damage_24_0_3_2.png", heartImage("ai","damage")},
  {"player_acquired_tray_378x130_0_3_2.png", baseTray},
  {"ai_status_panel_224x130_0_3_2.png", aiPanel},
}

for _,item in ipairs(runtime) do
  savePng(item[2], runtimeDir .. "/" .. item[1])
  savePng(item[2], outputDir .. "/" .. item[1])
end

makeHeartSource(sourceDir .. "/hp_hearts_24_0_3_2.aseprite")
makeTraySource(baseTray, cards, sourceDir .. "/player_acquired_tray_states_0_3_2.aseprite")

local stateSheet = Image(378*4,130,ColorMode.RGB)
stateSheet:clear(C.transparent)
for n=0,3 do paste(trayState(baseTray,cards,n),stateSheet,n*378,0) end
savePng(stateSheet, previewDir .. "/player_acquired_tray_states_0_3_2.png")
savePng(stateSheet, outputDir .. "/player_acquired_tray_states_0_3_2.png")

local pair = pairPreview(cardB,cardC)
savePng(pair, previewDir .. "/halli_exposed_pair_readability_0_3_2.png")
savePng(pair, outputDir .. "/halli_exposed_pair_readability_0_3_2.png")

local after = applicationPreview(screen,baseTray,aiPanel,hearts,cards)
savePng(after,previewDir .. "/halli_status_tray_application_preview_768x418_0_3_2.png")
savePng(after,outputDir .. "/halli_status_tray_application_preview_768x418_0_3_2.png")

local contact = Image(screen.width*2+4,screen.height,ColorMode.RGB)
contact:clear(C.ink)
paste(screen,contact,0,0)
fill(contact,screen.width,0,4,screen.height,C.brassHi)
paste(after,contact,screen.width+4,0)
savePng(contact,previewDir .. "/halli_status_tray_before_after_0_3_2.png")
savePng(contact,outputDir .. "/halli_status_tray_before_after_0_3_2.png")

print("Halli_StatusTray_0_3_2 generated")
