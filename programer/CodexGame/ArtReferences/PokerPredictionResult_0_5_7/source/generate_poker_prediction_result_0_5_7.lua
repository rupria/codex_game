-- Poker prediction/result UI 0.5.7 for issues #67 and #68.
-- Approved direction: no large rectangular modal, controls follow the round table,
-- community cards are capped at two, and insurance exposes only remaining charges.

local p = app.params
local outDir = assert(p.outDir)
local previewDir = assert(p.previewDir)
local sourceDir = assert(p.sourceDir)
local backgroundPath = assert(p.background)
local cardRoot = assert(p.cardRoot)
local cardBackPath = assert(p.cardBack)
local cratePath = assert(p.crate)

local C = {
  clear = Color{r=0,g=0,b=0,a=0},
  black = Color{r=5,g=5,b=5,a=255},
  panel = Color{r=22,g=15,b=11,a=255},
  leather = Color{r=55,g=31,b=18,a=255},
  leatherHi = Color{r=83,g=47,b=25,a=255},
  brassDark = Color{r=83,g=48,b=16,a=255},
  brass = Color{r=178,g=105,b=28,a=255},
  brassHi = Color{r=244,g=187,b=70,a=255},
  tealDark = Color{r=13,g=86,b=88,a=255},
  teal = Color{r=34,g=203,b=205,a=255},
  tealHi = Color{r=104,g=244,b=239,a=255},
  redDark = Color{r=100,g=28,b=31,a=255},
  red = Color{r=226,g=65,b=67,a=255},
  redHi = Color{r=255,g=119,b=104,a=255},
  cream = Color{r=239,g=221,b=181,a=255},
  iron = Color{r=91,g=88,b=80,a=255},
  disabled = Color{r=64,g=61,b=57,a=220}
}

local function load(path)
  local sprite = app.open(path)
  assert(sprite, 'cannot open '..path)
  local image = Image(sprite.cels[1].image)
  sprite:close()
  return image
end

local function save(image, path)
  local sprite = Sprite(image.width, image.height, ColorMode.RGB)
  sprite.cels[1].image:clear(C.clear)
  sprite.cels[1].image:drawImage(image, Point(0, 0))
  sprite:saveAs(path)
  sprite:close()
end

local function resize(src, width, height)
  local dst = Image(width, height, ColorMode.RGB)
  dst:clear(C.clear)
  for y=0,height-1 do
    local sy = math.min(src.height-1, math.floor(y*src.height/height))
    for x=0,width-1 do
      local sx = math.min(src.width-1, math.floor(x*src.width/width))
      dst:drawPixel(x, y, src:getPixel(sx, sy))
    end
  end
  return dst
end

local function fill(image, x, y, width, height, color)
  local x0 = math.max(0, math.floor(x))
  local y0 = math.max(0, math.floor(y))
  local x1 = math.min(image.width-1, math.floor(x+width-1))
  local y1 = math.min(image.height-1, math.floor(y+height-1))
  for yy=y0,y1 do
    for xx=x0,x1 do image:drawPixel(xx, yy, color) end
  end
end

local function hline(image, x0, x1, y, color)
  fill(image, x0, y, x1-x0+1, 1, color)
end

local function diamond(image, cx, cy, radius, color)
  for yy=-radius,radius do
    local span = radius-math.abs(yy)
    hline(image, cx-span, cx+span, cy+yy, color)
  end
end

local function roundedPanel(image, x, y, width, height, color)
  fill(image, x+6, y, width-12, height, color)
  fill(image, x+3, y+2, width-6, height-4, color)
  fill(image, x+1, y+5, width-2, height-10, color)
end

local function star(image, cx, cy, outer, inner, color)
  diamond(image, cx, cy, outer, color)
  fill(image, cx-inner, cy-outer-3, inner*2+1, outer*2+7, color)
  fill(image, cx-outer-3, cy-inner, outer*2+7, inner*2+1, color)
  diamond(image, cx, cy, inner, C.panel)
end

local function heart(image, cx, cy, color)
  diamond(image, cx-4, cy-3, 4, color)
  diamond(image, cx+4, cy-3, 4, color)
  for yy=0,8 do
    local span = 8-yy
    hline(image, cx-span, cx+span, cy+yy-1, color)
  end
end

local function arrow(image, cx, cy, direction, color)
  local sign = direction == 'up' and -1 or 1
  fill(image, cx-3, cy-1, 7, 12, color)
  for i=0,6 do
    local yy = cy + sign*(i-7)
    hline(image, cx-(6-i), cx+(6-i), yy, color)
  end
end

local function shieldShape(image, cx, cy, radius, color)
  for yy=0,radius*2 do
    local inset = yy < radius and 0 or math.floor((yy-radius+1)/2)
    hline(image, cx-radius+inset, cx+radius-inset, cy-radius+yy, color)
  end
  diamond(image, cx, cy+radius+1, math.max(1, math.floor(radius/3)), color)
end

local function shield(image, cx, cy, color, highlight)
  shieldShape(image, cx, cy, 9, C.black)
  shieldShape(image, cx, cy, 7, color)
  shieldShape(image, cx, cy, 4, C.leather)
  fill(image, cx-1, cy-4, 3, 9, highlight)
  diamond(image, cx, cy+5, 2, highlight)
end

local function shear(source, direction)
  local target = Image(source.width+12, source.height, ColorMode.RGB)
  target:clear(C.clear)
  for y=0,source.height-1 do
    local shift = math.floor((y-(source.height-1)/2)*0.18*direction)
    for x=0,source.width-1 do
      local color = source:getPixel(x,y)
      target:drawPixel(x+6+shift,y,color)
    end
  end
  return target
end

local function predictionTab(side, state)
  local width, height = 220, 64
  local image = Image(width, height, ColorMode.RGB)
  image:clear(C.clear)
  local accent = side == 'player' and C.teal or C.red
  local accentHi = side == 'player' and C.tealHi or C.redHi
  local accentDark = side == 'player' and C.tealDark or C.redDark
  if state == 'disabled' then
    accent, accentHi, accentDark = C.disabled, C.iron, C.black
  end

  roundedPanel(image, 0, 0, width, height, C.black)
  roundedPanel(image, 2, 2, width-4, height-4, C.brassDark)
  roundedPanel(image, 4, 4, width-8, height-8, C.leather)
  if state == 'hover' or state == 'selected' then
    roundedPanel(image, 7, 7, width-14, height-14, C.leatherHi)
  end
  fill(image, 8, 7, width-16, 3, state == 'selected' and accentHi or accent)
  fill(image, 8, height-10, width-16, 3, accentDark)
  diamond(image, 15, 32, 5, C.brass)
  diamond(image, width-16, 32, 5, C.brassDark)
  star(image, 34, 32, state == 'selected' and 10 or 8, 3, C.brassHi)
  arrow(image, 34, 32, side == 'player' and 'up' or 'down', accentHi)
  -- Text-safe region begins at x=62. Text remains runtime-localized.
  fill(image, 68, 29, 124, 3, C.cream)
  if state == 'selected' then
    fill(image, 12, 12, 3, height-24, accentHi)
    fill(image, width-15, 12, 3, height-24, accentHi)
  end
  return shear(image, side == 'player' and 1 or -1)
end

local function continueTab(state)
  local image = Image(164, 44, ColorMode.RGB)
  image:clear(C.clear)
  roundedPanel(image, 0, 0, 164, 44, C.black)
  roundedPanel(image, 2, 2, 160, 40, C.brassDark)
  roundedPanel(image, 4, 4, 156, 36, state == 'hover' and C.leatherHi or C.leather)
  diamond(image, 14, 22, 4, C.brassHi)
  diamond(image, 149, 22, 4, C.brass)
  fill(image, 36, 20, 92, 3, C.cream)
  return image
end

local states = {'idle','hover','selected','disabled'}
local generated = {player={}, ai={}}
for _,state in ipairs(states) do
  generated.player[state] = predictionTab('player', state)
  generated.ai[state] = predictionTab('ai', state)
  save(generated.player[state], outDir..'/poker_prediction_player_'..state..'_232x64_0_5_7.png')
  save(generated.ai[state], outDir..'/poker_prediction_ai_'..state..'_232x64_0_5_7.png')
end

local continueIdle = continueTab('idle')
local continueHover = continueTab('hover')
save(continueIdle, outDir..'/poker_result_continue_idle_164x44_0_5_7.png')
save(continueHover, outDir..'/poker_result_continue_hover_164x44_0_5_7.png')

local insuranceIcon = Image(28, 28, ColorMode.RGB)
insuranceIcon:clear(C.clear)
shield(insuranceIcon, 14, 13, C.brass, C.brassHi)
save(insuranceIcon, outDir..'/poker_insurance_remaining_icon_28_0_5_7.png')

local successIcon = Image(28, 28, ColorMode.RGB)
successIcon:clear(C.clear)
star(successIcon, 14, 14, 8, 3, C.brassHi)
fill(successIcon, 9, 14, 3, 6, C.teal)
fill(successIcon, 12, 18, 3, 3, C.teal)
fill(successIcon, 15, 11, 3, 8, C.teal)
save(successIcon, outDir..'/poker_prediction_success_icon_28_0_5_7.png')

local stageEmblem = Image(40, 40, ColorMode.RGB)
stageEmblem:clear(C.clear)
star(stageEmblem, 20, 20, 11, 4, C.brassHi)
save(stageEmblem, outDir..'/poker_prediction_stage_emblem_40_0_5_7.png')

-- Editable Aseprite source contact sheets.
local controls = Image(508, 300, ColorMode.RGB)
controls:clear(Color{r=9,g=7,b=6,a=255})
for index,state in ipairs(states) do
  controls:drawImage(generated.player[state], Point(8, 8+(index-1)*72))
  controls:drawImage(generated.ai[state], Point(260, 8+(index-1)*72))
end
save(controls, sourceDir..'/poker_prediction_controls_0_5_7.aseprite')

local statusSheet = Image(260, 88, ColorMode.RGB)
statusSheet:clear(Color{r=9,g=7,b=6,a=255})
statusSheet:drawImage(stageEmblem, Point(12, 24))
statusSheet:drawImage(insuranceIcon, Point(84, 30))
statusSheet:drawImage(successIcon, Point(144, 30))
statusSheet:drawImage(resize(continueIdle, 82, 22), Point(174, 33))
save(statusSheet, sourceDir..'/poker_prediction_status_icons_0_5_7.aseprite')

-- 960x540 integration preview using project-owned runtime art.
local preview = load(backgroundPath)
assert(preview.width == 960 and preview.height == 540, 'preview background must be 960x540')

local function card(path, x, y)
  local src = resize(load(path), 56, 78)
  fill(preview, x+4, y+5, 56, 78, Color{r=0,g=0,b=0,a=110})
  preview:drawImage(src, Point(x, y))
end

local back = resize(load(cardBackPath), 56, 78)
for index=0,2 do
  local x = 384 + index*68
  fill(preview, x+4, 85, 56, 78, Color{r=0,g=0,b=0,a=110})
  preview:drawImage(back, Point(x, 80))
end

-- Community is intentionally exactly two cards (hard maximum in the rules).
card(cardRoot..'/card_poker_spades_j.png', 416, 218)
card(cardRoot..'/card_poker_clubs_6.png', 488, 218)
card(cardRoot..'/card_poker_clubs_j.png', 380, 338)
card(cardRoot..'/card_poker_hearts_7.png', 452, 338)
card(cardRoot..'/card_poker_spades_q.png', 524, 338)

-- Standalone health hearts; no enclosing rectangular frame.
for index=0,2 do
  heart(preview, 294+index*28, 142, C.red)
  heart(preview, 294+index*28, 402, C.teal)
end

local crate = resize(load(cratePath), 88, 76)
preview:drawImage(crate, Point(650, 350))

preview:drawImage(stageEmblem, Point(460, 22))
fill(preview, 422, 72, 116, 3, C.cream)
preview:drawImage(insuranceIcon, Point(690, 112))
fill(preview, 724, 124, 92, 3, C.cream)
preview:drawImage(successIcon, Point(624, 428))
fill(preview, 660, 440, 100, 3, C.cream)

preview:drawImage(generated.player.selected, Point(139, 456))
preview:drawImage(generated.ai.idle, Point(589, 456))
preview:drawImage(continueIdle, Point(398, 490))
save(preview, previewDir..'/poker_prediction_result_round_table_preview_960x540_0_5_7.png')

local sheet = Image(960, 360, ColorMode.RGB)
sheet:clear(Color{r=9,g=7,b=6,a=255})
sheet:drawImage(generated.player.idle, Point(20, 20))
sheet:drawImage(generated.player.hover, Point(20, 100))
sheet:drawImage(generated.player.selected, Point(20, 180))
sheet:drawImage(generated.player.disabled, Point(20, 260))
sheet:drawImage(generated.ai.idle, Point(280, 20))
sheet:drawImage(generated.ai.hover, Point(280, 100))
sheet:drawImage(generated.ai.selected, Point(280, 180))
sheet:drawImage(generated.ai.disabled, Point(280, 260))
sheet:drawImage(stageEmblem, Point(560, 24))
sheet:drawImage(insuranceIcon, Point(568, 102))
sheet:drawImage(successIcon, Point(568, 174))
sheet:drawImage(continueIdle, Point(640, 92))
sheet:drawImage(continueHover, Point(640, 164))
save(sheet, previewDir..'/poker_prediction_result_asset_states_960x360_0_5_7.png')

print('PokerPredictionResult 0.5.7 art generated')
