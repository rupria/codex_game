local sourceRoot = app.params["source"]
local exportRoot = app.params["export"]
local unityRoot = app.params["unity"]
local reviewRoot = app.params["review"]
local sampleOnly = app.params["sample"] == "true"

if sourceRoot == nil or exportRoot == nil or unityRoot == nil or reviewRoot == nil then
  error("Required parameters: source, export, unity, review")
end

local CARD_WIDTH = 64
local CARD_HEIGHT = 90

local colors = {
  background = Color { r = 17, g = 20, b = 26, a = 255 },
  board = Color { r = 36, g = 43, b = 53, a = 255 },
  card = Color { r = 232, g = 225, b = 208, a = 255 },
  ink = Color { r = 26, g = 27, b = 32, a = 255 },
  cyan = Color { r = 81, g = 214, b = 229, a = 255 },
  gold = Color { r = 245, g = 196, b = 81, a = 255 },
  wrong = Color { r = 229, g = 83, b = 83, a = 255 },
  red = Color { r = 216, g = 74, b = 91, a = 255 },
  locked = Color { r = 57, g = 66, b = 78, a = 255 }
}

local palette = Palette(9)
palette:setColor(0, colors.background)
palette:setColor(1, colors.board)
palette:setColor(2, colors.card)
palette:setColor(3, colors.ink)
palette:setColor(4, colors.cyan)
palette:setColor(5, colors.gold)
palette:setColor(6, colors.wrong)
palette:setColor(7, colors.red)
palette:setColor(8, colors.locked)

local font = {
  ["0"] = { "01110", "10001", "10011", "10101", "11001", "10001", "01110" },
  ["1"] = { "00100", "01100", "00100", "00100", "00100", "00100", "01110" },
  ["2"] = { "11110", "00001", "00001", "01110", "10000", "10000", "11111" },
  ["3"] = { "11110", "00001", "00001", "01110", "00001", "00001", "11110" },
  ["4"] = { "10010", "10010", "10010", "11111", "00010", "00010", "00010" },
  ["5"] = { "11111", "10000", "10000", "11110", "00001", "00001", "11110" },
  ["6"] = { "01111", "10000", "10000", "11110", "10001", "10001", "01110" },
  ["7"] = { "11111", "00001", "00010", "00100", "01000", "01000", "01000" },
  ["8"] = { "01110", "10001", "10001", "01110", "10001", "10001", "01110" },
  ["9"] = { "01110", "10001", "10001", "01111", "00001", "00001", "11110" },
  ["A"] = { "01110", "10001", "10001", "11111", "10001", "10001", "10001" },
  ["J"] = { "00111", "00010", "00010", "00010", "00010", "10010", "01100" },
  ["Q"] = { "01110", "10001", "10001", "10001", "10101", "10010", "01101" },
  ["K"] = { "10001", "10010", "10100", "11000", "10100", "10010", "10001" }
}

local suits = {
  { id = "spades", color = colors.ink, accent = colors.cyan, pattern = { "00100", "01110", "11111", "11111", "00100", "01110", "00000" } },
  { id = "diamonds", color = colors.red, accent = colors.red, pattern = { "00100", "01110", "11111", "01110", "00100", "00000", "00000" } },
  { id = "hearts", color = colors.red, accent = colors.red, pattern = { "01010", "11111", "11111", "01110", "00100", "00000", "00000" } },
  { id = "clubs", color = colors.ink, accent = colors.cyan, pattern = { "00100", "01110", "10101", "01110", "00100", "01110", "00000" } }
}

local ranks = {
  { id = "a", label = "A" },
  { id = "k", label = "K" },
  { id = "q", label = "Q" },
  { id = "j", label = "J" },
  { id = "10", label = "10" },
  { id = "9", label = "9" },
  { id = "8", label = "8" },
  { id = "7", label = "7" },
  { id = "6", label = "6" },
  { id = "5", label = "5" },
  { id = "4", label = "4" },
  { id = "3", label = "3" },
  { id = "2", label = "2" }
}

local function put(image, x, y, color)
  if x >= 0 and y >= 0 and x < image.width and y < image.height then
    image:drawPixel(x, y, color)
  end
end

local function fillRect(image, x, y, width, height, color)
  for py = y, y + height - 1 do
    for px = x, x + width - 1 do
      put(image, px, py, color)
    end
  end
end

local function drawPattern(image, x, y, pattern, color, scale)
  scale = scale or 1
  for row = 1, #pattern do
    local line = pattern[row]
    for column = 1, #line do
      if string.sub(line, column, column) == "1" then
        fillRect(image, x + (column - 1) * scale, y + (row - 1) * scale, scale, scale, color)
      end
    end
  end
end

local function drawText(image, text, x, y, color)
  local cursor = x
  for index = 1, #text do
    local character = string.sub(text, index, index)
    local pattern = font[character]
    if pattern ~= nil then
      drawPattern(image, cursor, y, pattern, color, 1)
      cursor = cursor + 6
    end
  end
end

local function drawCardShape(image, fillColor)
  for y = 0, CARD_HEIGHT - 1 do
    local inset = 0
    if y == 0 or y == CARD_HEIGHT - 1 then
      inset = 4
    elseif y == 1 or y == CARD_HEIGHT - 2 then
      inset = 2
    else
      inset = 1
    end
    fillRect(image, inset, y, CARD_WIDTH - inset * 2, 1, colors.ink)
  end

  for y = 2, CARD_HEIGHT - 3 do
    local inset = 3
    if y == 2 or y == CARD_HEIGHT - 3 then
      inset = 5
    end
    fillRect(image, inset, y, CARD_WIDTH - inset * 2, 1, fillColor)
  end

  put(image, 6, 3, colors.gold)
  put(image, CARD_WIDTH - 7, CARD_HEIGHT - 4, colors.gold)
end

local function drawDiamond(image, centerX, centerY, radius, color)
  for dy = -radius, radius do
    local halfWidth = radius - math.abs(dy)
    fillRect(image, centerX - halfWidth, centerY + dy, halfWidth * 2 + 1, 1, color)
  end
end

local function drawSkullMarker(image, x, y, accent)
  local centerX = x + 8
  local centerY = y + 8
  drawDiamond(image, centerX, centerY, 8, colors.ink)
  drawDiamond(image, centerX, centerY, 7, accent)
  drawDiamond(image, centerX, centerY, 5, colors.ink)

  local skull = {
    "0111110",
    "1111111",
    "1101011",
    "1101011",
    "1111111",
    "0111110",
    "0011100"
  }
  drawPattern(image, x + 5, y + 4, skull, colors.card, 1)
  put(image, x + 7, y + 7, colors.ink)
  put(image, x + 11, y + 7, colors.ink)
  put(image, x + 9, y + 10, colors.ink)
  put(image, x + 8, y + 11, colors.ink)
  put(image, x + 10, y + 11, colors.ink)
end

local function drawSkullCount(image, count, accent, offsetX, offsetY)
  if count == 1 then
    drawSkullMarker(image, offsetX + 18, offsetY + 11, accent)
  elseif count == 2 then
    drawSkullMarker(image, offsetX + 8, offsetY + 11, accent)
    drawSkullMarker(image, offsetX + 27, offsetY + 11, accent)
  else
    drawSkullMarker(image, offsetX + 18, offsetY + 1, accent)
    drawSkullMarker(image, offsetX + 8, offsetY + 20, accent)
    drawSkullMarker(image, offsetX + 27, offsetY + 20, accent)
  end
end

local function drawSuit(image, suit, x, y)
  drawPattern(image, x, y, suit.pattern, suit.color, 1)
end

local function renderCardFace(suit, rank, skullCount)
  local image = Image(CARD_WIDTH, CARD_HEIGHT, ColorMode.RGB)
  image:clear()
  drawCardShape(image, colors.card)
  drawText(image, rank.label, 5, 6, suit.color)
  drawSuit(image, suit, 7, 16)
  drawSkullCount(image, skullCount, suit.accent, 6, 27)

  drawSuit(image, suit, 51, 74)
  put(image, 31, 78, colors.gold)
  put(image, 30, 79, colors.gold)
  put(image, 31, 79, colors.gold)
  put(image, 32, 79, colors.gold)
  put(image, 31, 80, colors.gold)
  return image
end

local function renderCardFrontBase()
  local image = Image(CARD_WIDTH, CARD_HEIGHT, ColorMode.RGB)
  image:clear()
  drawCardShape(image, colors.card)
  return image
end

local function renderCardBack()
  local image = Image(CARD_WIDTH, CARD_HEIGHT, ColorMode.RGB)
  image:clear()
  drawCardShape(image, colors.locked)

  for x = 7, 56 do
    put(image, x, 7, colors.cyan)
    put(image, x, 82, colors.cyan)
  end
  for y = 7, 82 do
    put(image, 7, y, colors.cyan)
    put(image, 56, y, colors.cyan)
  end

  for offset = 0, 6 do
    put(image, 10 + offset, 11 + offset, colors.cyan)
    put(image, 53 - offset, 11 + offset, colors.cyan)
    put(image, 10 + offset, 78 - offset, colors.cyan)
    put(image, 53 - offset, 78 - offset, colors.cyan)
  end

  fillRect(image, 24, 34, 16, 2, colors.gold)
  fillRect(image, 22, 36, 20, 2, colors.gold)
  fillRect(image, 20, 38, 24, 14, colors.gold)
  fillRect(image, 22, 38, 20, 13, colors.ink)
  fillRect(image, 24, 52, 16, 2, colors.gold)
  fillRect(image, 29, 54, 6, 3, colors.gold)

  local skull = {
    "0111110",
    "1111111",
    "1101011",
    "1101011",
    "1111111",
    "0111110",
    "0011100"
  }
  drawPattern(image, 28, 40, skull, colors.cyan, 1)
  put(image, 30, 43, colors.ink)
  put(image, 34, 43, colors.ink)
  put(image, 32, 46, colors.ink)

  drawDiamond(image, 32, 25, 3, colors.gold)
  drawDiamond(image, 32, 65, 3, colors.gold)
  return image
end

local function makeSprite(image, sourcePath, exportPath, unityPath, layerName)
  local sprite = Sprite(image.width, image.height, ColorMode.RGB)
  sprite:setPalette(palette)
  sprite.layers[1].name = layerName or "art"
  sprite.cels[1].image:drawImage(image, Point(0, 0))
  sprite:saveAs(sourcePath)
  sprite:saveCopyAs(exportPath)
  if unityPath ~= nil then
    sprite:saveCopyAs(unityPath)
  end
  sprite:close()
end

local function componentPaths(name)
  return app.fs.joinPath(sourceRoot, "components", name .. ".aseprite"),
    app.fs.joinPath(exportRoot, "components", name .. ".png"),
    app.fs.joinPath(unityRoot, "components", name .. ".png")
end

local function variantPaths(name)
  return app.fs.joinPath(sourceRoot, "deck_variants", name .. ".aseprite"),
    app.fs.joinPath(exportRoot, "deck_variants", name .. ".png"),
    app.fs.joinPath(unityRoot, "deck_variants", name .. ".png")
end

local function saveComponent(name, image)
  local sourcePath, exportPath, unityPath = componentPaths(name)
  makeSprite(image, sourcePath, exportPath, unityPath, name)
end

local function renderSuitComponent(suit)
  local image = Image(9, 9, ColorMode.RGB)
  image:clear()
  drawSuit(image, suit, 2, 1)
  return image
end

local function renderRankComponent(rank, color)
  local width = #rank.label * 6 - 1
  local image = Image(width, 7, ColorMode.RGB)
  image:clear()
  drawText(image, rank.label, 0, 0, color)
  return image
end

local function renderSkullComponent(count)
  local image = Image(52, 40, ColorMode.RGB)
  image:clear()
  drawSkullCount(image, count, colors.cyan, 0, 0)
  return image
end

local function writeText(path, content)
  local file = assert(io.open(path, "wb"))
  file:write(content)
  file:close()
end

local catalogEntries = {}
local csvRows = { "asset_id,suit,rank,skull_count,filename,unity_asset_path" }

local function addCatalogEntry(suit, rank, skullCount, filename)
  local unityPath = "Assets/Art/Prototype/Cards/deck_variants/" .. filename .. ".png"
  table.insert(catalogEntries,
    string.format('    {"assetId":"%s","suit":"%s","rank":"%s","skullCount":%d,"assetPath":"%s"}',
      filename, suit.id, rank.label, skullCount, unityPath))
  table.insert(csvRows,
    string.format("%s,%s,%s,%d,%s.png,%s", filename, suit.id, rank.label, skullCount, filename, unityPath))
end

saveComponent("card_front_base", renderCardFrontBase())
saveComponent("card_back", renderCardBack())
for _, suit in ipairs(suits) do
  saveComponent("suit_" .. suit.id, renderSuitComponent(suit))
end
for _, rank in ipairs(ranks) do
  saveComponent("rank_" .. rank.id, renderRankComponent(rank, colors.ink))
end
for skullCount = 1, 3 do
  saveComponent(string.format("skull_%02d", skullCount), renderSkullComponent(skullCount))
end

local sampleKeys = {
  ["spades:a:1"] = true,
  ["hearts:10:2"] = true,
  ["diamonds:k:3"] = true
}

for _, suit in ipairs(suits) do
  for _, rank in ipairs(ranks) do
    for skullCount = 1, 3 do
      local key = suit.id .. ":" .. rank.id .. ":" .. skullCount
      if not sampleOnly or sampleKeys[key] then
        local filename = string.format("card_%s_%s_skull_%02d", suit.id, rank.id, skullCount)
        local sourcePath, exportPath, unityPath = variantPaths(filename)
        makeSprite(renderCardFace(suit, rank, skullCount), sourcePath, exportPath, unityPath, filename)
        addCatalogEntry(suit, rank, skullCount, filename)
      end
    end
  end
end

local catalogJson = table.concat({
  "{",
  '  "specRevision": "gameplay_flow_0.05",',
  '  "cardWidth": 64,',
  '  "cardHeight": 90,',
  '  "cardBackAssetPath": "Assets/Art/Prototype/Cards/components/card_back.png",',
  '  "skullAssignment": "runtime_defined_use_1_2_3_variant",',
  '  "cards": [',
  table.concat(catalogEntries, ",\n"),
  "  ]",
  "}",
  ""
}, "\n")

writeText(app.fs.joinPath(exportRoot, "card_art_catalog.json"), catalogJson)
writeText(app.fs.joinPath(unityRoot, "card_art_catalog.json"), catalogJson)
writeText(app.fs.joinPath(exportRoot, "card_art_catalog.csv"), table.concat(csvRows, "\n") .. "\n")

local preview = Image(285, 100, ColorMode.RGB)
preview:clear(Rectangle(0, 0, preview.width, preview.height), colors.background)
fillRect(preview, 0, 2, preview.width, 96, colors.board)
preview:drawImage(renderCardFace(suits[1], ranks[1], 1), Point(5, 5))
preview:drawImage(renderCardFace(suits[3], ranks[5], 2), Point(75, 5))
preview:drawImage(renderCardFace(suits[2], ranks[2], 3), Point(145, 5))
preview:drawImage(renderCardBack(), Point(215, 5))
makeSprite(
  preview,
  app.fs.joinPath(sourceRoot, "card_preview_0_05.aseprite"),
  app.fs.joinPath(reviewRoot, "card_preview_0_05.png"),
  nil,
  "preview")

print(string.format("Generated %d card variants (sample=%s)", #catalogEntries, tostring(sampleOnly)))
