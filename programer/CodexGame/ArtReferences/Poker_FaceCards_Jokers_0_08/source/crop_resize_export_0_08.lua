-- Aseprite batch export helper for the approved western J/Q/K and Joker set.
-- Required params: input, x, y, width, height, highres, runtime112, runtime56, aseprite

local p = app.params
local input = assert(p.input, "input is required")
assert(p.x, "x is required")
assert(p.y, "y is required")
assert(p.width, "width is required")
assert(p.height, "height is required")
local x = tonumber(p.x)
local y = tonumber(p.y)
local width = tonumber(p.width)
local height = tonumber(p.height)
local highres = assert(p.highres, "highres is required")
local runtime112 = assert(p.runtime112, "runtime112 is required")
local runtime56 = assert(p.runtime56, "runtime56 is required")
local aseprite = assert(p.aseprite, "aseprite is required")

local spr = assert(app.open(input), "cannot open " .. input)
spr:crop(x, y, width, height)
spr:saveAs(highres)
spr:saveAs(aseprite)

spr:resize{
  width = 112,
  height = 156,
  method = "bilinear"
}
spr:saveAs(runtime112)

spr:resize{
  width = 56,
  height = 78,
  method = "bilinear"
}
spr:saveAs(runtime56)
spr:close()
