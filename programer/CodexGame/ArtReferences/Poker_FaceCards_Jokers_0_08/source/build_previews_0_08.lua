-- Builds compact review sheets from the approved source sheets and Joker cards.

local p = app.params
local sourceDir = assert(p.sourceDir, "sourceDir is required")
local highresDir = assert(p.highresDir, "highresDir is required")
local previewDir = assert(p.previewDir, "previewDir is required")

local function loadImage(path)
  local spr = assert(app.open(path), "cannot open " .. path)
  local img = Image(spr.cels[1].image)
  spr:close()
  return img
end

local function resizeNearest(src, width, height)
  local dst = Image(width, height, ColorMode.RGB)
  dst:clear(Color{r=0, g=0, b=0, a=0})
  for y = 0, height - 1 do
    local sy = math.min(src.height - 1, math.floor(y * src.height / height))
    for x = 0, width - 1 do
      local sx = math.min(src.width - 1, math.floor(x * src.width / width))
      dst:drawPixel(x, y, src:getPixel(sx, sy))
    end
  end
  return dst
end

local function savePng(img, path)
  local spr = Sprite(img.width, img.height, ColorMode.RGB)
  spr.cels[1].image:drawImage(img, Point(0, 0))
  spr:saveAs(path)
  spr:close()
end

local bg = Color{r=12, g=14, b=15, a=255}
local face = Image(864, 496, ColorMode.RGB)
face:clear(bg)
local sheets = {
  "clubs_jqk_approved.png",
  "diamonds_jqk_approved.png",
  "hearts_jqk_approved.png",
  "spades_jqk_approved.png"
}
for i, name in ipairs(sheets) do
  local img = resizeNearest(loadImage(sourceDir .. "/approved_sheets/" .. name), 416, 228)
  local x = ((i - 1) % 2) * 424 + 12
  local y = math.floor((i - 1) / 2) * 236 + 12
  face:drawImage(img, Point(x, y))
end
savePng(face, previewDir .. "/face_cards_12_preview_0_08.png")

local joker = Image(672, 526, ColorMode.RGB)
joker:clear(bg)
local sheriff = resizeNearest(loadImage(highresDir .. "/card_poker_joker_brass_sheriff_revolver.png"), 318, 494)
local cardsharp = resizeNearest(loadImage(highresDir .. "/card_poker_joker_crimson_cardsharp.png"), 318, 494)
joker:drawImage(sheriff, Point(12, 16))
joker:drawImage(cardsharp, Point(342, 16))
savePng(joker, previewDir .. "/joker_pair_preview_0_08.png")
