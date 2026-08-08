local params = app.params

local function required(name)
  local value = params[name]
  if value == nil or value == "" then error("Missing --script-param " .. name) end
  return value
end

local closedSourcePath = required("closedSource")
local openSourcePath = required("openSource")
local shopBackgroundPath = required("shopBackground")
local outDir = required("outDir")
local W, H = 960, 540

local function rgba(r, g, b, a)
  return app.pixelColor.rgba(r, g, b, a or 255)
end

local function canvas(w, h)
  return Image(w or W, h or H, ColorMode.RGB)
end

local function loadImage(path)
  local source = app.open(path)
  if source == nil or #source.cels == 0 then error("Unable to load " .. path) end
  local image = source.cels[1].image:clone()
  source:close()
  return image
end

local function resizeNearest(source, newW, newH)
  local result = canvas(newW, newH)
  for y = 0, newH - 1 do
    local sy = math.min(source.height - 1, math.floor(y * source.height / newH))
    for x = 0, newW - 1 do
      local sx = math.min(source.width - 1, math.floor(x * source.width / newW))
      result:putPixel(x, y, source:getPixel(sx, sy))
    end
  end
  return result
end

local function zoomCrop(source, scale)
  local zw = math.floor(source.width * scale + 0.5)
  local zh = math.floor(source.height * scale + 0.5)
  local zoomed = resizeNearest(source, zw, zh)
  local result = canvas(source.width, source.height)
  result:drawImage(zoomed, Point(math.floor((source.width - zw) / 2), math.floor((source.height - zh) / 2)))
  return result
end

local function fillRect(image, x, y, w, h, color)
  for py = math.max(0, y), math.min(image.height - 1, y + h - 1) do
    for px = math.max(0, x), math.min(image.width - 1, x + w - 1) do
      image:putPixel(px, py, color)
    end
  end
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
      local nx, ny = (px + 0.5 - cx) / rx, (py + 0.5 - cy) / ry
      if nx * nx + ny * ny <= 1 then image:putPixel(px, py, color) end
    end
  end
end

local function pointInPolygon(x, y, points)
  local inside = false
  local j = #points
  for i = 1, #points do
    local xi, yi = points[i][1], points[i][2]
    local xj, yj = points[j][1], points[j][2]
    local intersects = ((yi > y) ~= (yj > y)) and
      (x < (xj - xi) * (y - yi) / ((yj - yi) == 0 and 0.0001 or (yj - yi)) + xi)
    if intersects then inside = not inside end
    j = i
  end
  return inside
end

local function fillPolygon(image, points, color)
  local minX, maxX, minY, maxY = image.width - 1, 0, image.height - 1, 0
  for _, p in ipairs(points) do
    minX = math.min(minX, p[1]); maxX = math.max(maxX, p[1])
    minY = math.min(minY, p[2]); maxY = math.max(maxY, p[2])
  end
  for y = math.max(0, minY), math.min(image.height - 1, maxY) do
    for x = math.max(0, minX), math.min(image.width - 1, maxX) do
      if pointInPolygon(x + 0.5, y + 0.5, points) then image:putPixel(x, y, color) end
    end
  end
end

local darkWood = rgba(39, 22, 12, 255)
local midWood = rgba(76, 41, 19, 255)
local woodHi = rgba(121, 67, 29, 255)
local brass = rgba(178, 116, 43, 255)
local brassHi = rgba(235, 179, 67, 255)
local dust = rgba(156, 104, 58, 150)
local dustDim = rgba(104, 68, 39, 90)
local nearBlack = rgba(2, 2, 3, 255)

local function doorPoints(side, width)
  if side == "left" then
    return {{3, 42}, {width - 12, 9}, {width - 2, 26}, {width - 2, 188}, {width - 14, 204}, {3, 188}}
  end
  return {{127 - width + 12, 9}, {124, 42}, {124, 188}, {127 - width + 14, 204}, {127 - width + 2, 188}, {127 - width + 2, 26}}
end

local function drawDoor(side, state)
  local image = canvas(128, 210)
  local widths = {118, 92, 56, 24}
  local width = widths[state]
  local points = doorPoints(side, width)
  fillPolygon(image, points, darkWood)
  for i = 1, #points do
    local j = i == #points and 1 or i + 1
    drawLine(image, points[i][1], points[i][2], points[j][1], points[j][2], woodHi, 3)
  end
  local minX = side == "left" and 6 or (128 - width + 6)
  local maxX = side == "left" and (width - 7) or 121
  local usable = math.max(4, maxX - minX)
  for row = 0, 5 do
    local y = 58 + row * 22
    local inset = math.floor(row / 2)
    drawLine(image, minX + inset, y, maxX - inset, y, midWood, 3)
    if usable > 38 then drawLine(image, minX + 12, y + 5, maxX - 10, y + 5, woodHi, 1) end
  end
  local hingeX = side == "left" and 1 or 119
  fillRect(image, hingeX, 72, 8, 14, brass)
  fillRect(image, hingeX, 156, 8, 14, brass)
  fillRect(image, hingeX + (side == "left" and 2 or 0), 75, 4, 8, brassHi)
  fillRect(image, hingeX + (side == "left" and 2 or 0), 159, 4, 8, brassHi)
  return image
end

local function doorOverlay(state, scale)
  local overlay = canvas(W, H)
  overlay:drawImage(drawDoor("left", state), Point(368, 105))
  overlay:drawImage(drawDoor("right", state), Point(464, 105))
  if scale ~= nil and scale ~= 1 then return zoomCrop(overlay, scale) end
  return overlay
end

local function compositeDoors(base, state, scale)
  local result = base:clone()
  result:drawImage(doorOverlay(state, scale), Point(0, 0))
  return result
end

local function drawDustFrame(frameIndex)
  local image = canvas(96, 64)
  local specs = {
    {{42,46,14,7},{51,42,8,5}},
    {{30,41,25,10},{50,35,18,9},{62,45,13,7}},
    {{16,34,28,13},{42,27,26,14},{66,38,18,10}},
    {{6,29,24,12},{31,18,25,15},{60,25,25,13},{78,38,12,8}}
  }
  for i, s in ipairs(specs[frameIndex]) do
    fillEllipse(image, s[1], s[2], s[3], s[4], i % 2 == 0 and dustDim or dust)
  end
  return image
end

local function drawVignette(strength)
  local image = canvas(W, H)
  local edgeMax = 170
  for y = 0, H - 1 do
    for x = 0, W - 1 do
      local d = math.min(x, W - 1 - x, y, H - 1 - y)
      if d < edgeMax then
        local ratio = 1 - d / edgeMax
        local alpha = math.floor(255 * strength * ratio * ratio)
        if alpha > 0 then image:putPixel(x, y, rgba(0, 0, 0, alpha)) end
      end
    end
  end
  return image
end

local function drawSpinner(frameIndex)
  local image = canvas(64, 64)
  local cx, cy, radius = 32, 32, 22
  for i = 0, 7 do
    local angle = math.rad(-90 + i * 45)
    local x = math.floor(cx + math.cos(angle) * radius - 3)
    local y = math.floor(cy + math.sin(angle) * radius - 3)
    local distance = (i - (frameIndex - 1)) % 8
    local color = distance == 0 and brassHi or (distance <= 2 and brass or rgba(79, 57, 35, 180))
    local size = distance == 0 and 8 or 6
    fillEllipse(image, x, y, size, size, color)
  end
  fillEllipse(image, 21, 21, 22, 22, rgba(10, 8, 6, 230))
  fillEllipse(image, 27, 24, 4, 5, brass)
  fillEllipse(image, 34, 24, 4, 5, brass)
  fillRect(image, 29, 31, 7, 3, brass)
  fillRect(image, 30, 34, 2, 4, brass)
  fillRect(image, 34, 34, 2, 4, brass)
  return image
end

local function addBlackOverlay(image, alpha)
  local overlay = canvas(image.width, image.height)
  fillRect(overlay, 0, 0, image.width, image.height, rgba(0, 0, 0, alpha))
  local result = image:clone()
  result:drawImage(overlay, Point(0, 0))
  return result
end

local closedBackground = resizeNearest(loadImage(closedSourcePath), W, H)
local openBackground = resizeNearest(loadImage(openSourcePath), W, H)
local shopBackground = resizeNearest(loadImage(shopBackgroundPath), W, H)

closedBackground:saveAs(outDir .. "/stage_exit_background_closed_unlit_960x540_0_3_1.png")
openBackground:saveAs(outDir .. "/stage_exit_background_open_unlit_960x540_0_3_1.png")

for state = 1, 4 do
  drawDoor("left", state):saveAs(outDir .. string.format("/stage_exit_door_left_%02d_128x210_0_3_1.png", state))
  drawDoor("right", state):saveAs(outDir .. string.format("/stage_exit_door_right_%02d_128x210_0_3_1.png", state))
  drawDustFrame(state):saveAs(outDir .. string.format("/stage_exit_walk_dust_%02d_96x64_0_3_1.png", state))
end

local vignette = drawVignette(0.76)
vignette:saveAs(outDir .. "/stage_exit_walk_vignette_960x540_0_3_1.png")
local fadeTile = canvas(16, 16)
fillRect(fadeTile, 0, 0, 16, 16, nearBlack)
fadeTile:saveAs(outDir .. "/stage_transition_fade_black_16_0_3_1.png")

for frameIndex = 1, 8 do
  drawSpinner(frameIndex):saveAs(outDir .. string.format("/stage_transition_loading_%02d_64_0_3_1.png", frameIndex))
end

local doorSheet = canvas(1024, 230)
fillRect(doorSheet, 0, 0, 1024, 230, rgba(4, 3, 3, 255))
for state = 1, 4 do
  local x = (state - 1) * 256
  doorSheet:drawImage(drawDoor("left", state), Point(x, 10))
  doorSheet:drawImage(drawDoor("right", state), Point(x + 128, 10))
end
doorSheet:saveAs(outDir .. "/stage_exit_door_states_contact_sheet_1024x230_0_3_1.png")

local spinnerSheet = canvas(576, 80)
fillRect(spinnerSheet, 0, 0, 576, 80, nearBlack)
for frameIndex = 1, 8 do spinnerSheet:drawImage(drawSpinner(frameIndex), Point(8 + (frameIndex - 1) * 70, 8)) end
spinnerSheet:saveAs(outDir .. "/stage_transition_loading_contact_sheet_576x80_0_3_1.png")

local previewFrames = {}
previewFrames[1] = shopBackground:clone()
previewFrames[2] = closedBackground:clone()
previewFrames[3] = zoomCrop(closedBackground, 1.08)
previewFrames[3]:drawImage(drawVignette(0.24), Point(0, 0))
previewFrames[4] = compositeDoors(zoomCrop(openBackground, 1.12), 2, 1.12)
previewFrames[4]:drawImage(drawDustFrame(2), Point(432, 456))
previewFrames[4]:drawImage(drawVignette(0.34), Point(0, 0))
previewFrames[5] = compositeDoors(zoomCrop(openBackground, 1.20), 4, 1.20)
previewFrames[5]:drawImage(drawDustFrame(4), Point(432, 446))
previewFrames[5]:drawImage(drawVignette(0.46), Point(0, 0))
previewFrames[6] = addBlackOverlay(previewFrames[5], 190)
previewFrames[7] = canvas(W, H)
fillRect(previewFrames[7], 0, 0, W, H, nearBlack)
previewFrames[7]:drawImage(drawSpinner(1), Point(448, 238))
previewFrames[8] = canvas(W, H)
fillRect(previewFrames[8], 0, 0, W, H, nearBlack)
previewFrames[8]:drawImage(drawSpinner(5), Point(448, 238))

previewFrames[2]:saveAs(outDir .. "/stage_transition_preview_exit_closed_960x540_0_3_1.png")
previewFrames[5]:saveAs(outDir .. "/stage_transition_preview_door_open_960x540_0_3_1.png")
previewFrames[7]:saveAs(outDir .. "/stage_transition_preview_loading_960x540_0_3_1.png")

local storyboard = canvas(1920, 720)
fillRect(storyboard, 0, 0, 1920, 720, nearBlack)
local indices = {1, 2, 3, 4, 5, 7}
for i = 1, 6 do
  local thumb = resizeNearest(previewFrames[indices[i]], 640, 360)
  local x = ((i - 1) % 3) * 640
  local y = math.floor((i - 1) / 3) * 360
  storyboard:drawImage(thumb, Point(x, y))
  fillRect(storyboard, x, y, 640, 6, brass)
end
storyboard:saveAs(outDir .. "/stage_transition_storyboard_1920x720_0_3_1.png")

local sprite = Sprite(W, H, ColorMode.RGB)
sprite.filename = outDir .. "/stage_transition_storyboard_960x540_0_3_1.aseprite"
for _ = 2, 8 do sprite:newEmptyFrame() end
local durations = {0.22, 0.32, 0.65, 0.18, 0.28, 0.25, 0.09, 0.09}
for i = 1, 8 do sprite.frames[i].duration = durations[i] end

local shopLayer = sprite.layers[1]; shopLayer.name = "01_Shop_Unlit_Reference"
local exitLayer = sprite:newLayer(); exitLayer.name = "02_Exit_Backgrounds_Unlit"
local doorLayer = sprite:newLayer(); doorLayer.name = "03_Swing_Door_States"
local fxLayer = sprite:newLayer(); fxLayer.name = "04_Walk_Dust_And_Vignette"
local fadeLayer = sprite:newLayer(); fadeLayer.name = "05_Fade_To_Black"
local loadingLayer = sprite:newLayer(); loadingLayer.name = "06_Loading_Indicator_No_Text"

sprite:newCel(shopLayer, sprite.frames[1], shopBackground:clone(), Point(0, 0))
sprite:newCel(exitLayer, sprite.frames[2], closedBackground:clone(), Point(0, 0))
sprite:newCel(exitLayer, sprite.frames[3], zoomCrop(closedBackground, 1.08), Point(0, 0))
sprite:newCel(fxLayer, sprite.frames[3], drawVignette(0.24), Point(0, 0))
sprite:newCel(exitLayer, sprite.frames[4], zoomCrop(openBackground, 1.12), Point(0, 0))
sprite:newCel(doorLayer, sprite.frames[4], doorOverlay(2, 1.12), Point(0, 0))
local frame4Fx = canvas(W, H)
frame4Fx:drawImage(drawDustFrame(2), Point(432, 456))
frame4Fx:drawImage(drawVignette(0.34), Point(0, 0))
sprite:newCel(fxLayer, sprite.frames[4], frame4Fx, Point(0, 0))
sprite:newCel(exitLayer, sprite.frames[5], zoomCrop(openBackground, 1.20), Point(0, 0))
sprite:newCel(doorLayer, sprite.frames[5], doorOverlay(4, 1.20), Point(0, 0))
local frame5Fx = canvas(W, H)
frame5Fx:drawImage(drawDustFrame(4), Point(432, 446))
frame5Fx:drawImage(drawVignette(0.46), Point(0, 0))
sprite:newCel(fxLayer, sprite.frames[5], frame5Fx, Point(0, 0))
sprite:newCel(fadeLayer, sprite.frames[6], previewFrames[6]:clone(), Point(0, 0))
sprite:newCel(loadingLayer, sprite.frames[7], previewFrames[7]:clone(), Point(0, 0))
sprite:newCel(loadingLayer, sprite.frames[8], previewFrames[8]:clone(), Point(0, 0))

sprite:saveAs(sprite.filename)
sprite:saveCopyAs(outDir .. "/stage_transition_storyboard_960x540_0_3_1.gif")
sprite:close()

local loadingSprite = Sprite(64, 64, ColorMode.RGB)
loadingSprite.filename = outDir .. "/stage_transition_loading_64_0_3_1.aseprite"
for _ = 2, 8 do loadingSprite:newEmptyFrame() end
loadingSprite.layers[1].name = "01_Brass_Loading_Indicator"
for i = 1, 8 do
  loadingSprite.frames[i].duration = 0.09
  loadingSprite:newCel(loadingSprite.layers[1], loadingSprite.frames[i], drawSpinner(i), Point(0, 0))
end
loadingSprite:saveAs(loadingSprite.filename)
loadingSprite:saveCopyAs(outDir .. "/stage_transition_loading_64_0_3_1.gif")
loadingSprite:close()
