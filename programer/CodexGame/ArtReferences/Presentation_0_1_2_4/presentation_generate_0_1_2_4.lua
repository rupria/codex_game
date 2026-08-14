local params = app.params

local function required(name)
  local value = params[name]
  if value == nil or value == "" then error("Missing --script-param " .. name) end
  return value
end

local runtimeDir = required("runtimeDir")
local sourceDir = required("sourceDir")
local previewDir = required("previewDir")
local saloonBackgroundPath = required("saloonBackground")

local function rgba(r, g, b, a)
  return app.pixelColor.rgba(r, g, b, a or 255)
end

local function canvas(w, h)
  return Image(w, h, ColorMode.RGB)
end

local function fillRect(image, x, y, w, h, color)
  for py = math.max(0, y), math.min(image.height - 1, y + h - 1) do
    for px = math.max(0, x), math.min(image.width - 1, x + w - 1) do
      image:putPixel(px, py, color)
    end
  end
end

local function strokeRect(image, x, y, w, h, color, thickness)
  thickness = thickness or 1
  fillRect(image, x, y, w, thickness, color)
  fillRect(image, x, y + h - thickness, w, thickness, color)
  fillRect(image, x, y, thickness, h, color)
  fillRect(image, x + w - thickness, y, thickness, h, color)
end

local function drawLine(image, x0, y0, x1, y1, color, thickness)
  thickness = thickness or 1
  local dx = math.abs(x1 - x0)
  local sx = x0 < x1 and 1 or -1
  local dy = -math.abs(y1 - y0)
  local sy = y0 < y1 and 1 or -1
  local err = dx + dy
  while true do
    fillRect(image, x0 - math.floor(thickness / 2), y0 - math.floor(thickness / 2), thickness, thickness, color)
    if x0 == x1 and y0 == y1 then break end
    local e2 = 2 * err
    if e2 >= dy then err = err + dy; x0 = x0 + sx end
    if e2 <= dx then err = err + dx; y0 = y0 + sy end
  end
end

local function fillEllipse(image, x, y, w, h, color)
  local rx, ry = w / 2, h / 2
  local cx, cy = x + rx, y + ry
  for py = math.max(0, y), math.min(image.height - 1, y + h - 1) do
    for px = math.max(0, x), math.min(image.width - 1, x + w - 1) do
      local nx = (px + 0.5 - cx) / rx
      local ny = (py + 0.5 - cy) / ry
      if nx * nx + ny * ny <= 1 then image:putPixel(px, py, color) end
    end
  end
end

local function drawDiamond(image, cx, cy, radius, color)
  for y = -radius, radius do
    local half = radius - math.abs(y)
    fillRect(image, cx - half, cy + y, half * 2 + 1, 1, color)
  end
end

local function drawCornerFrame(image, x, y, w, h, outer, inner, accent)
  strokeRect(image, x + 4, y + 4, w - 8, h - 8, outer, 2)
  drawLine(image, x + 9, y + 8, x + w - 10, y + 8, inner, 1)
  local c = math.max(7, math.floor(math.min(w, h) * 0.09))
  drawLine(image, x + 2, y + c, x + 2, y + 2, accent, 2)
  drawLine(image, x + 2, y + 2, x + c, y + 2, accent, 2)
  drawLine(image, x + w - c - 1, y + 2, x + w - 3, y + 2, accent, 2)
  drawLine(image, x + w - 3, y + 2, x + w - 3, y + c, accent, 2)
  drawLine(image, x + 2, y + h - c - 1, x + 2, y + h - 3, accent, 2)
  drawLine(image, x + 2, y + h - 3, x + c, y + h - 3, accent, 2)
  drawLine(image, x + w - c - 1, y + h - 3, x + w - 3, y + h - 3, accent, 2)
  drawLine(image, x + w - 3, y + h - c - 1, x + w - 3, y + h - 3, accent, 2)
end

local function drawCardOutline(image, x, y, w, h, color, fill)
  if fill ~= nil then fillRect(image, x + 2, y + 2, w - 4, h - 4, fill) end
  strokeRect(image, x, y, w, h, color, 2)
  fillRect(image, x, y, 3, 3, rgba(0, 0, 0, 0))
  fillRect(image, x + w - 3, y, 3, 3, rgba(0, 0, 0, 0))
  fillRect(image, x, y + h - 3, 3, 3, rgba(0, 0, 0, 0))
  fillRect(image, x + w - 3, y + h - 3, 3, 3, rgba(0, 0, 0, 0))
end

local nearBlack = rgba(4, 7, 10, 255)
local panel = rgba(18, 18, 17, 242)
local panelDim = rgba(15, 17, 18, 214)
local teal = rgba(67, 188, 192, 255)
local tealDim = rgba(37, 102, 105, 255)
local red = rgba(202, 65, 71, 255)
local redDim = rgba(100, 42, 45, 255)
local gold = rgba(172, 116, 43, 255)
local goldHi = rgba(225, 181, 84, 255)
local brassDark = rgba(74, 47, 24, 255)
local steel = rgba(86, 91, 91, 255)
local steelDim = rgba(44, 48, 48, 255)
local cream = rgba(244, 226, 184, 255)
local blackAlpha = rgba(0, 0, 0, 180)
local violet = rgba(149, 106, 218, 255)

local function drawRivet(image, x, y, bright)
  fillRect(image, x, y, 3, 3, brassDark)
  image:putPixel(x + 1, y, bright or gold)
end

local function drawIronPanel(image, x, y, w, h, active)
  fillRect(image, x + 2, y + 3, w - 4, h - 6, rgba(19, 19, 18, 238))
  strokeRect(image, x, y, w, h, rgba(34, 30, 25, 255), 2)
  drawLine(image, x + 6, y + 3, x + w - 7, y + 3, active or steel, 1)
  drawLine(image, x + 7, y + h - 4, x + w - 8, y + h - 4, rgba(8, 8, 8, 255), 1)
  drawRivet(image, x + 7, y + 7, gold)
  drawRivet(image, x + w - 10, y + 7, gold)
  drawRivet(image, x + 7, y + h - 10, brassDark)
  drawRivet(image, x + w - 10, y + h - 10, brassDark)
  for i = 0, math.floor(w / 52) - 1 do
    drawLine(image, x + 20 + i * 47, y + 16 + (i % 3) * 9, x + 31 + i * 47, y + 14 + (i % 3) * 9, rgba(65, 59, 49, 110), 1)
  end
end

local function drawCornerClips(image, x, y, w, h, color, length)
  length = length or 16
  drawLine(image, x, y + length, x, y, color, 3)
  drawLine(image, x, y, x + length, y, color, 3)
  drawLine(image, x + w - length, y, x + w, y, color, 3)
  drawLine(image, x + w, y, x + w, y + length, color, 3)
  drawLine(image, x, y + h - length, x, y + h, color, 3)
  drawLine(image, x, y + h, x + length, y + h, color, 3)
  drawLine(image, x + w - length, y + h, x + w, y + h, color, 3)
  drawLine(image, x + w, y + h - length, x + w, y + h, color, 3)
end

local function save(image, name)
  image:saveAs(runtimeDir .. "/" .. name)
end

local function drawBell(image, cx, cy, color)
  fillEllipse(image, cx - 12, cy - 12, 24, 18, color)
  fillRect(image, cx - 15, cy + 4, 30, 4, color)
  fillRect(image, cx - 10, cy + 9, 20, 3, color)
  fillRect(image, cx - 3, cy - 17, 6, 6, color)
end

local function phaseIcon(kind)
  local image = canvas(64, 64)
  fillEllipse(image, 3, 3, 58, 58, rgba(12, 12, 11, 245))
  fillEllipse(image, 6, 6, 52, 52, rgba(34, 29, 22, 255))
  fillEllipse(image, 9, 9, 46, 46, rgba(11, 14, 15, 255))
  for i = 0, 7 do
    local a = math.rad(i * 45)
    drawRivet(image, 30 + math.floor(math.cos(a) * 27), 30 + math.floor(math.sin(a) * 27), i == 0 and goldHi or gold)
  end
  if kind == "three_call" then
    drawCardOutline(image, 13, 13, 14, 23, teal, panel)
    drawCardOutline(image, 37, 13, 14, 23, red, panel)
    drawBell(image, 32, 40, goldHi)
  else
    drawCardOutline(image, 22, 12, 14, 22, steel, panel)
    drawCardOutline(image, 29, 15, 14, 22, gold, panel)
    drawLine(image, 13, 47, 25, 40, teal, 2)
    drawLine(image, 51, 47, 39, 40, red, 2)
    drawDiamond(image, 32, 47, 4, goldHi)
  end
  return image
end

local function labelFrame()
  local image = canvas(288, 80)
  drawIronPanel(image, 12, 17, 264, 46, gold)
  drawCornerClips(image, 8, 13, 272, 54, gold, 12)
  drawDiamond(image, 24, 40, 3, goldHi)
  drawDiamond(image, 264, 40, 3, goldHi)
  return image
end

local function skipButton(state)
  local image = canvas(120, 44)
  local border = state == "hover" and goldHi or gold
  drawIronPanel(image, 2, 4, 116, 36, state == "pressed" and brassDark or border)
  for i = 0, 1 do
    local ox = 43 + i * 19
    drawLine(image, ox, 13, ox + 11, 22, cream, 2)
    drawLine(image, ox + 11, 22, ox, 31, cream, 2)
  end
  return image
end

local function opponentFrame()
  local image = canvas(360, 152)
  drawIronPanel(image, 8, 10, 344, 132, steel)
  drawCornerClips(image, 4, 6, 352, 140, gold, 15)
  fillRect(image, 20, 22, 108, 108, rgba(7, 8, 8, 255))
  strokeRect(image, 20, 22, 108, 108, rgba(65, 59, 49, 255), 2)
  fillEllipse(image, 52, 35, 44, 44, rgba(2, 3, 3, 255))
  fillRect(image, 38, 73, 72, 43, rgba(2, 3, 3, 255))
  drawLine(image, 38, 48, 112, 48, gold, 3)
  drawLine(image, 49, 41, 99, 29, gold, 2)
  drawLine(image, 151, 35, 326, 35, steel, 2)
  drawLine(image, 151, 62, 292, 62, steelDim, 2)
  drawLine(image, 151, 76, 254, 76, steelDim, 1)
  for i = 0, 2 do
    fillEllipse(image, 153 + i * 29, 99, 13, 13, i == 0 and goldHi or brassDark)
    drawRivet(image, 158 + i * 29, 104, gold)
  end
  return image
end

local function focusVignette()
  local image = canvas(960, 540)
  local edge = 170
  for y = 0, 539 do
    for x = 0, 959 do
      local d = math.min(x, 959 - x, y, 539 - y)
      if d < edge then
        local r = 1 - d / edge
        local a = math.floor(210 * r * r)
        image:putPixel(x, y, rgba(0, 0, 0, a))
      end
    end
  end
  return image
end

local function tinyLock(image, x, y, color)
  strokeRect(image, x + 4, y, 12, 12, color, 3)
  fillRect(image, x, y + 9, 20, 16, color)
  fillRect(image, x + 8, y + 15, 4, 7, nearBlack)
end

local function bulletToken(image, x, y, color, empty)
  if empty then
    strokeRect(image, x, y, 10, 22, color, 2)
  else
    fillRect(image, x + 2, y, 6, 5, goldHi)
    fillRect(image, x, y + 5, 10, 13, color)
    fillRect(image, x + 1, y + 18, 8, 4, brassDark)
  end
end

local function penaltyIcon(kind)
  local image = canvas(64, 64)
  fillEllipse(image, 3, 3, 58, 58, panel)
  strokeRect(image, 8, 8, 48, 48, brassDark, 2)
  drawCornerFrame(image, 5, 5, 54, 54, brassDark, gold, goldHi)
  if kind == "limit_1" then
    bulletToken(image, 27, 21, gold, false)
  elseif kind == "limit_2" then
    bulletToken(image, 20, 21, gold, false)
    bulletToken(image, 34, 21, gold, false)
  elseif kind == "used_1" then
    bulletToken(image, 27, 21, steel, true)
    drawLine(image, 18, 45, 46, 17, red, 3)
  elseif kind == "exhausted" then
    bulletToken(image, 18, 24, steelDim, true)
    bulletToken(image, 32, 24, steelDim, true)
    tinyLock(image, 22, 18, steel)
  elseif kind == "inventory" then
    strokeRect(image, 15, 21, 34, 25, tealDim, 3)
    drawLine(image, 22, 21, 27, 15, tealDim, 3)
    drawLine(image, 27, 15, 37, 15, tealDim, 3)
    drawLine(image, 37, 15, 42, 21, tealDim, 3)
    tinyLock(image, 33, 29, steel)
  else
    drawCardOutline(image, 13, 16, 22, 31, cream, panel)
    drawCardOutline(image, 25, 20, 22, 31, gold, panel)
    tinyLock(image, 34, 31, steel)
  end
  return image
end

local function penaltyLabelFrame()
  local image = canvas(320, 84)
  drawIronPanel(image, 8, 13, 304, 58, steel)
  drawCornerClips(image, 4, 9, 312, 66, gold, 11)
  drawLine(image, 78, 29, 292, 29, steel, 2)
  drawLine(image, 78, 50, 241, 50, steelDim, 2)
  return image
end

local function transitionOverlay(kind)
  local image = canvas(960, 540)
  if kind == "desaturate" then
    fillRect(image, 0, 0, 960, 540, rgba(20, 32, 39, 122))
    fillRect(image, 160, 72, 640, 396, rgba(0, 0, 0, 26))
  else
    fillRect(image, 0, 0, 960, 540, rgba(0, 0, 0, 156))
    for y = 120, 430 do
      local half = math.floor(300 * math.sqrt(math.max(0, 1 - ((y - 275) / 155) ^ 2)))
      fillRect(image, 480 - half, y, half * 2, 1, rgba(0, 0, 0, 0))
    end
    drawLine(image, 150, 350, 375, 430, tealDim, 3)
    drawLine(image, 810, 350, 585, 430, redDim, 3)
    drawDiamond(image, 480, 430, 5, gold)
  end
  return image
end

local function cardStateFrame(state)
  local image = canvas(124, 172)
  local color = steel
  if state == "hover" then color = cream end
  if state == "selected" then color = goldHi end
  if state == "confirmed" then color = teal end
  if state == "disabled" then color = steelDim end
  drawCornerClips(image, 5, 5, 114, 162, color, state == "idle" and 12 or 17)
  if state ~= "idle" then
    drawLine(image, 24, 8, 100, 8, rgba(app.pixelColor.rgbaR(color), app.pixelColor.rgbaG(color), app.pixelColor.rgbaB(color), 128), 1)
    drawLine(image, 24, 164, 100, 164, rgba(app.pixelColor.rgbaR(color), app.pixelColor.rgbaG(color), app.pixelColor.rgbaB(color), 128), 1)
  end
  if state == "selected" or state == "confirmed" then
    drawDiamond(image, 62, 7, 4, color)
    drawDiamond(image, 62, 165, 4, color)
  end
  if state == "disabled" then
    fillRect(image, 10, 10, 104, 152, rgba(16, 18, 18, 118))
    tinyLock(image, 52, 73, steel)
  end
  return image
end

local function motionTrail(color, reverse)
  local image = canvas(320, 96)
  for i = 0, 5 do
    local x0 = reverse and 282 - i * 26 or 38 + i * 26
    local x1 = reverse and x0 - 66 or x0 + 66
    local a = 205 - i * 26
    drawLine(image, x0, 18 + i * 9, x1, 18 + i * 9, rgba(app.pixelColor.rgbaR(color), app.pixelColor.rgbaG(color), app.pixelColor.rgbaB(color), a), 2)
  end
  return image
end

local function radialFx(color, frame, count)
  local image = canvas(64, 64)
  local cx, cy = 32, 32
  local radius = 8 + frame * 5
  local alpha = math.max(0, 255 - frame * 38)
  for i = 0, count - 1 do
    local angle = math.rad(i * (360 / count) + frame * 7)
    local x0 = math.floor(cx + math.cos(angle) * (radius - 5))
    local y0 = math.floor(cy + math.sin(angle) * (radius - 5))
    local x1 = math.floor(cx + math.cos(angle) * (radius + 8))
    local y1 = math.floor(cy + math.sin(angle) * (radius + 8))
    drawLine(image, x0, y0, x1, y1, rgba(app.pixelColor.rgbaR(color), app.pixelColor.rgbaG(color), app.pixelColor.rgbaB(color), alpha), frame < 3 and 3 or 2)
  end
  if frame <= 2 then fillEllipse(image, 26, 26, 12, 12, rgba(255, 244, 193, 190 - frame * 45)) end
  return image
end

local function fxSheet(color, count)
  local image = canvas(384, 64)
  for frame = 0, 5 do image:drawImage(radialFx(color, frame, count), Point(frame * 64, 0)) end
  return image
end

local function rewardPanel()
  local image = canvas(720, 360)
  drawIronPanel(image, 12, 36, 696, 288, steel)
  drawCornerClips(image, 6, 30, 708, 300, gold, 18)
  fillRect(image, 28, 67, 664, 208, rgba(5, 8, 9, 205))
  drawLine(image, 35, 64, 685, 64, steelDim, 1)
  for i = 0, 4 do drawRivet(image, 103 + i * 128, 47, i == 2 and goldHi or gold) end
  drawIronPanel(image, 242, 298, 236, 34, gold)
  return image
end

local function showdownFrame()
  local image = canvas(760, 420)
  drawIronPanel(image, 68, 27, 624, 28, redDim)
  drawIronPanel(image, 68, 365, 624, 28, tealDim)
  drawLine(image, 92, 41, 668, 41, redDim, 2)
  drawLine(image, 92, 379, 668, 379, tealDim, 2)
  drawCornerClips(image, 8, 8, 744, 404, steel, 28)
  for i = 0, 2 do
    drawDiamond(image, 320 + i * 60, 41, 4, red)
    drawDiamond(image, 320 + i * 60, 379, 4, teal)
  end
  return image
end

local function handLockedFrame()
  local image = canvas(420, 180)
  for i = 0, 2 do
    local x = 66 + i * 102
    fillRect(image, x + 4, 28, 76, 112, rgba(7, 9, 9, 96))
    drawCornerClips(image, x, 24, 84, 118, i == 1 and gold or steel, i == 1 and 16 or 11)
  end
  tinyLock(image, 200, 137, gold)
  return image
end

local function highlightFrame()
  local image = canvas(420, 96)
  drawIronPanel(image, 12, 29, 396, 38, gold)
  drawCornerClips(image, 6, 23, 408, 50, goldHi, 12)
  for i = 0, 7 do drawDiamond(image, 46 + i * 47, 48, 2 + (i % 2), (i == 3 or i == 4) and goldHi or gold) end
  return image
end

local function resultFrame(kind)
  local w, h = kind == "clear" and 560 or 720, kind == "clear" and 240 or 360
  local image = canvas(w, h)
  drawIronPanel(image, 10, 12, w - 20, h - 24, steel)
  drawCornerClips(image, 4, 6, w - 8, h - 12, gold, 18)
  if kind == "clear" then
    drawDiamond(image, math.floor(w / 2), 48, 14, goldHi)
    for i = 0, 2 do fillEllipse(image, 170 + i * 105, 108, 36, 36, i == 1 and teal or gold) end
    drawLine(image, 116, 188, 444, 188, steelDim, 2)
  else
    drawDiamond(image, math.floor(w / 2), 44, 8, goldHi)
    drawCornerClips(image, 44, 83, w - 88, 112, steel, 13)
    drawCornerClips(image, 44, 221, w - 88, 72, steelDim, 10)
    drawIronPanel(image, 250, 308, 220, 30, gold)
  end
  return image
end

local function hpFlash(color)
  local image = canvas(96, 96)
  for frame = 0, 3 do
    local radius = 12 + frame * 9
    local alpha = 220 - frame * 48
    for i = 0, 7 do
      local a = math.rad(i * 45 + 22.5)
      drawLine(image, 48 + math.floor(math.cos(a) * radius), 48 + math.floor(math.sin(a) * radius), 48 + math.floor(math.cos(a) * (radius + 14)), 48 + math.floor(math.sin(a) * (radius + 14)), rgba(app.pixelColor.rgbaR(color), app.pixelColor.rgbaG(color), app.pixelColor.rgbaB(color), alpha), 2)
    end
  end
  drawLine(image, 40, 30, 53, 43, cream, 3)
  drawLine(image, 53, 43, 45, 55, cream, 3)
  drawLine(image, 45, 55, 58, 69, cream, 3)
  return image
end

save(phaseIcon("three_call"), "phase_three_call_icon_64_0_1_2_4.png")
save(phaseIcon("showdown"), "phase_showdown_icon_64_0_1_2_4.png")
save(labelFrame(), "phase_entry_label_frame_288x80_0_1_2_4.png")
save(skipButton("idle"), "stage_entry_skip_button_idle_120x44_0_1_2_4.png")
save(skipButton("hover"), "stage_entry_skip_button_hover_120x44_0_1_2_4.png")
save(skipButton("pressed"), "stage_entry_skip_button_pressed_120x44_0_1_2_4.png")
save(opponentFrame(), "stage_entry_opponent_intro_frame_360x152_0_1_2_4.png")
save(focusVignette(), "stage_entry_table_focus_vignette_960x540_0_1_2_4.png")
save(penaltyIcon("limit_1"), "stage_item_limit_one_64_0_1_2_4.png")
save(penaltyIcon("limit_2"), "stage_item_limit_two_64_0_1_2_4.png")
save(penaltyIcon("used_1"), "stage_item_limit_used_one_64_0_1_2_4.png")
save(penaltyIcon("exhausted"), "stage_item_limit_exhausted_64_0_1_2_4.png")
save(penaltyIcon("inventory"), "stage_item_inventory_restricted_64_0_1_2_4.png")
save(penaltyIcon("card"), "stage_item_card_restricted_64_0_1_2_4.png")
save(penaltyLabelFrame(), "stage_item_penalty_label_frame_320x84_0_1_2_4.png")
save(transitionOverlay("desaturate"), "phase_transition_desaturate_overlay_960x540_0_1_2_4.png")
save(transitionOverlay("focus"), "phase_transition_focus_mask_960x540_0_1_2_4.png")
for _, state in ipairs({"idle", "hover", "selected", "confirmed", "disabled"}) do
  save(cardStateFrame(state), "private_card_candidate_" .. state .. "_124x172_0_1_2_4.png")
end
save(motionTrail(teal, false), "card_acquire_trail_player_320x96_0_1_2_4.png")
save(motionTrail(red, true), "card_acquire_trail_ai_320x96_0_1_2_4.png")
save(fxSheet(goldHi, 12), "bell_correct_glint_6f_384x64_0_1_2_4.png")
save(fxSheet(red, 8), "bell_wrong_impact_6f_384x64_0_1_2_4.png")
save(rewardPanel(), "wrong_reward_scroll_panel_720x360_0_1_2_4.png")
save(cardStateFrame("idle"), "wrong_reward_card_idle_124x172_0_1_2_4.png")
save(cardStateFrame("selected"), "wrong_reward_card_selected_124x172_0_1_2_4.png")
save(cardStateFrame("disabled"), "wrong_reward_card_locked_124x172_0_1_2_4.png")
save(showdownFrame(), "showdown_wide_frame_760x420_0_1_2_4.png")
save(handLockedFrame(), "showdown_hand_locked_frame_420x180_0_1_2_4.png")
save(highlightFrame(), "result_best_hand_highlight_420x96_0_1_2_4.png")
save(resultFrame("summary"), "result_summary_frame_720x360_0_1_2_4.png")
save(resultFrame("clear"), "stage_clear_frame_560x240_0_1_2_4.png")
save(hpFlash(teal), "hp_damage_flash_player_96_0_1_2_4.png")
save(hpFlash(red), "hp_damage_flash_ai_96_0_1_2_4.png")

local saloon = app.open(saloonBackgroundPath)
local saloonImage = saloon.cels[1].image:clone()
saloon:close()

local entryPreview = saloonImage:clone()
entryPreview:drawImage(focusVignette(), Point(0, 0))
entryPreview:drawImage(opponentFrame(), Point(300, 184))
entryPreview:drawImage(skipButton("idle"), Point(814, 474))
entryPreview:drawImage(phaseIcon("three_call"), Point(448, 70))
entryPreview:saveAs(previewDir .. "/presentation_entry_preview_960x540_0_1_2_4.png")

local transitionPreview = saloonImage:clone()
transitionPreview:drawImage(transitionOverlay("desaturate"), Point(0, 0))
transitionPreview:drawImage(labelFrame(), Point(336, 34))
transitionPreview:drawImage(phaseIcon("showdown"), Point(448, 42))
transitionPreview:drawImage(cardStateFrame("selected"), Point(298, 328))
transitionPreview:drawImage(cardStateFrame("confirmed"), Point(418, 328))
transitionPreview:drawImage(cardStateFrame("disabled"), Point(538, 328))
transitionPreview:drawImage(penaltyLabelFrame(), Point(16, 20))
transitionPreview:drawImage(penaltyIcon("limit_2"), Point(32, 30))
transitionPreview:saveAs(previewDir .. "/presentation_transition_preview_960x540_0_1_2_4.png")

local showdownPreview = saloonImage:clone()
showdownPreview:drawImage(showdownFrame(), Point(100, 74))
showdownPreview:drawImage(handLockedFrame(), Point(270, 176))
showdownPreview:drawImage(highlightFrame(), Point(270, 374))
showdownPreview:saveAs(previewDir .. "/presentation_showdown_preview_960x540_0_1_2_4.png")

local source = Sprite(960, 540, ColorMode.RGB)
source.filename = sourceDir .. "/presentation_states_960x540_0_1_2_4.aseprite"
for _ = 2, 3 do source:newEmptyFrame() end
source.frames[1].duration = 0.45
source.frames[2].duration = 2.00
source.frames[3].duration = 0.45
local bgLayer = source.layers[1]; bgLayer.name = "01_Saloon_Reference_Unlit"
local phaseLayer = source:newLayer(); phaseLayer.name = "02_Phase_Icons_And_Frames"
local stateLayer = source:newLayer(); stateLayer.name = "03_Penalty_And_Selection_States"
local fxLayer = source:newLayer(); fxLayer.name = "04_Transition_And_Result_FX"
for i = 1, 3 do source:newCel(bgLayer, source.frames[i], saloonImage:clone(), Point(0, 0)) end
source:newCel(phaseLayer, source.frames[1], entryPreview, Point(0, 0))
source:newCel(stateLayer, source.frames[2], transitionPreview, Point(0, 0))
source:newCel(fxLayer, source.frames[3], showdownPreview, Point(0, 0))
source:saveAs(source.filename)
source:close()
