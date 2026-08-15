-- Places stage-opponent portraits into the existing presentation frame.
-- Required params: base, input1..input4, outputDir, aseprite
-- Optional param: variant (defaults to poster)

local p = app.params
assert(p.base, "base is required")
assert(p.input1, "input1 is required")
assert(p.input2, "input2 is required")
assert(p.input3, "input3 is required")
assert(p.input4, "input4 is required")
assert(p.outputDir, "outputDir is required")
assert(p.aseprite, "aseprite is required")
local variant = p.variant or "poster"

local inputs = {p.input1, p.input2, p.input3, p.input4}
local baseSprite = assert(app.open(p.base), "cannot open " .. p.base)
local baseImage = baseSprite.cels[1].image:clone()
baseSprite:close()

local source = Sprite(960, 540, ColorMode.RGB)
source.layers[1].name = "stage_entry_" .. variant .. "_opponents_1_to_4"
for _ = 2, 4 do source:newEmptyFrame() end

for i,path in ipairs(inputs) do
  local portraitSprite = assert(app.open(path), "cannot open " .. path)
  local preview = baseImage:clone()
  preview:drawImage(portraitSprite.cels[1].image, Point(320, 206))
  portraitSprite:close()
  preview:saveAs(p.outputDir .. string.format("/stage_entry_opponent_%02d_%s_preview_960x540_0_1_2_4.png", i, variant))
  source:newCel(source.layers[1], source.frames[i], preview, Point(0, 0))
  source.frames[i].duration = 0.8
end

source:saveAs(p.aseprite)
source:close()
