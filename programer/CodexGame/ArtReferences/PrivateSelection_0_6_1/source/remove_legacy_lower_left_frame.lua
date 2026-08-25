local sourcePath = app.params["source"]
local outputPath = app.params["output"]

if sourcePath == nil or sourcePath == "" then
  error("Missing --script-param source=<png>")
end
if outputPath == nil or outputPath == "" then
  error("Missing --script-param output=<png>")
end

local sprite = app.open(sourcePath)
if sprite == nil then
  error("Unable to open source image: " .. sourcePath)
end
if sprite.width ~= 860 or sprite.height ~= 456 then
  error("Expected 860x456 modal panel, got " .. sprite.width .. "x" .. sprite.height)
end

local image = sprite.cels[1].image
local panelBackground = Color { r = 14, g = 13, b = 11, a = 255 }

-- PrivateSelection_0_6_0 baked an obsolete button frame into the modal panel.
-- Runtime 0.6.0+ draws SelectionCountPanel and ConfirmVisual independently, so
-- keeping this frame produces the duplicated lower-left confirm seen in QA.
for y = 318, 363 do
  for x = 18, 207 do
    image:drawPixel(x, y, panelBackground)
  end
end

sprite:saveCopyAs(outputPath)
sprite:close()
