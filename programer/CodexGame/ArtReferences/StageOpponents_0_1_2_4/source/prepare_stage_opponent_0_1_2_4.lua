-- Aseprite preparation helper for stage-opponent portraits.
-- Required params: input, reference512, source256, runtime108

local p = app.params
local input = assert(p.input, "input is required")
local reference512 = assert(p.reference512, "reference512 is required")
local source256 = assert(p.source256, "source256 is required")
local runtime108 = assert(p.runtime108, "runtime108 is required")

local spr = assert(app.open(input), "cannot open " .. input)
spr:resize{
  width = 512,
  height = 512,
  method = "bilinear"
}
spr:saveAs(reference512)

spr:resize{
  width = 256,
  height = 256,
  method = "bilinear"
}
spr:saveAs(source256)

spr:resize{
  width = 108,
  height = 108,
  method = "bilinear"
}
spr:saveAs(runtime108)
spr:close()
