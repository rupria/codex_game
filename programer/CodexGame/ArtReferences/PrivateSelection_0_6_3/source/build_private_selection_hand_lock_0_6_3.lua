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

local function sourceYFor(outputY)
  if outputY < 16 then return outputY end
  if outputY >= 104 then return outputY - 56 end
  return 32
end

local function writeVariant(path, fill, accent)
  local sourceSprite = app.open(buttonSource)
  if sourceSprite == nil then error("Unable to open button source") end
  if sourceSprite.width ~= 184 or sourceSprite.height ~= 64 then
    error("Expected 184x64 button source")
  end
  local sourceImage = sourceSprite.cels[1].image
  local targetSprite = Sprite(184, 120, ColorMode.RGB)
  local targetImage = targetSprite.cels[1].image
  for y = 0, 119 do
    local sourceY = sourceYFor(y)
    for x = 0, 183 do
      local value = sourceImage:getPixel(x, sourceY)
      if value == sourceFill then
        value = fill
      elseif value == sourceAccent then
        value = accent
      end
      targetImage:drawPixel(x, y, value)
    end
  end
  targetSprite:saveCopyAs(path)
  targetSprite:close()
  sourceSprite:close()
end

writeVariant(outputIdle, pixel.rgba(23, 17, 13, 255), pixel.rgba(180, 108, 27, 255))
writeVariant(outputHover, pixel.rgba(20, 92, 98, 255), pixel.rgba(244, 188, 69, 255))
writeVariant(outputActive, pixel.rgba(7, 62, 68, 255), pixel.rgba(27, 199, 210, 255))
writeVariant(outputDisabled, pixel.rgba(32, 30, 27, 255), pixel.rgba(82, 79, 72, 255))

local modalSprite = app.open(modalSource)
local idleSprite = app.open(outputIdle)
if modalSprite == nil or idleSprite == nil then
  error("Unable to open preview sources")
end
modalSprite.cels[1].image:drawImage(idleSprite.cels[1].image, Point(22, 324))
modalSprite:saveCopyAs(outputPreview)
idleSprite:close()
modalSprite:close()
