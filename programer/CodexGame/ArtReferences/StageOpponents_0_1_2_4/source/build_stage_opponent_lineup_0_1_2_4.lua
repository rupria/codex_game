-- Builds a compact visual-QA lineup from four runtime portraits.
-- Required params: input1..input4, output, aseprite

local p = app.params
assert(p.input1, "input1 is required")
assert(p.input2, "input2 is required")
assert(p.input3, "input3 is required")
assert(p.input4, "input4 is required")
local inputs = {p.input1, p.input2, p.input3, p.input4}
local output = assert(p.output, "output is required")
local aseprite = assert(p.aseprite, "aseprite is required")

local C = {
  bg = Color{r=12,g=10,b=8,a=255},
  panel = Color{r=24,g=19,b=14,a=255},
  border = Color{r=144,g=93,b=30,a=255},
  highlight = Color{r=224,g=162,b=57,a=255},
  shadow = Color{r=3,g=3,b=3,a=255}
}

local function fill(img,x,y,w,h,c)
  for yy=y,y+h-1 do for xx=x,x+w-1 do img:drawPixel(xx,yy,c) end end
end

local function stroke(img,x,y,w,h,c,t)
  t=t or 1
  fill(img,x,y,w,t,c); fill(img,x,y+h-t,w,t,c)
  fill(img,x,y,t,h,c); fill(img,x+w-t,y,t,h,c)
end

local function nearest(src,w,h)
  local dst=Image(w,h,ColorMode.RGB)
  for y=0,h-1 do
    local sy=math.min(src.height-1,math.floor(y*src.height/h))
    for x=0,w-1 do
      local sx=math.min(src.width-1,math.floor(x*src.width/w))
      dst:drawPixel(x,y,src:getPixel(sx,sy))
    end
  end
  return dst
end

local board = Image(960,264,ColorMode.RGB)
board:clear(C.bg)
for i,path in ipairs(inputs) do
  local spr=assert(app.open(path),"cannot open " .. path)
  local portrait=nearest(spr.cels[1].image,208,208)
  local x=24+(i-1)*240
  fill(board,x,20,216,224,C.shadow)
  fill(board,x+4,16,208,224,C.panel)
  stroke(board,x+4,16,208,224,C.border,3)
  stroke(board,x+8,20,200,216,C.highlight,1)
  board:drawImage(portrait,Point(x+4,20))
  fill(board,x+8,232,200,4,C.shadow)
  spr:close()
end

local out=Sprite(board.width,board.height,ColorMode.RGB)
out.layers[1].name="stage_opponent_lineup_1_to_4"
out.cels[1].image:drawImage(board,Point(0,0))
out:saveAs(output)
out:saveAs(aseprite)
out:close()
