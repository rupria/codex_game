local modalSource = app.params["modal"]
local buttonSource = app.params["button"]
local outputIdle = app.params["idle"]
local outputHover = app.params["hover"]
local outputActive = app.params["active"]
local outputDisabled = app.params["disabled"]
local outputPreview = app.params["preview"]

for name, value in pairs({
  modal = modalSource,
  button = buttonSource,
  idle = outputIdle,
  hover = outputHover,
  active = outputActive,
  disabled = outputDisabled,
  preview = outputPreview
}) do
  if value == nil or value == "" then
    error("Missing --script-param " .. name .. "=<path>")
  end
end

local pixel = app.pixelColor
local sourceFill = pixel.rgba(23, 17, 13, 255)
local sourceAccent = pixel.rgba(180, 108, 27, 255)

local function writeVariant(path, fill, accent)
  local sprite = app.open(buttonSource)
  if sprite == nil then error("Unable to open button source") end
  if sprite.width ~= 184 or sprite.height ~= 64 then
    error("Expected 184x64 button source")
  end
  local image = sprite.cels[1].image
  for y = 0, image.height - 1 do
    for x = 0, image.width - 1 do
      local value = image:getPixel(x, y)
      if value == sourceFill then
        image:drawPixel(x, y, fill)
      elseif value == sourceAccent then
        image:drawPixel(x, y, accent)
      end
    end
  end
  sprite:saveCopyAs(path)
  sprite:close()
end

writeVariant(outputIdle, pixel.rgba(7, 62, 68, 255), pixel.rgba(27, 199, 210, 255))
writeVariant(outputHover, pixel.rgba(20, 92, 98, 255), pixel.rgba(244, 188, 69, 255))
writeVariant(outputActive, pixel.rgba(7, 62, 68, 255), pixel.rgba(244, 188, 69, 255))
writeVariant(outputDisabled, pixel.rgba(32, 30, 27, 255), pixel.rgba(82, 79, 72, 255))

local modalSprite = app.open(modalSource)
local idleSprite = app.open(outputIdle)
if modalSprite == nil or idleSprite == nil then
  error("Unable to open preview sources")
end
if modalSprite.width ~= 860 or modalSprite.height ~= 456 then
  error("Expected 860x456 modal source")
end
modalSprite.cels[1].image:drawImage(idleSprite.cels[1].image, Point(22, 370))
modalSprite:saveCopyAs(outputPreview)
idleSprite:close()
modalSprite:close()
