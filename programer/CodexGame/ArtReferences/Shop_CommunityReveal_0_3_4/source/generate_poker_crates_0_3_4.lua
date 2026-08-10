-- Poker item crates 0.3.4
-- Converts approved ImageGen art into three Aseprite-authored Unity sprites.

local p=app.params
local sourceImage=assert(p.sourceImage)
local sourceDir=assert(p.sourceDir)
local previewDir=assert(p.previewDir)
local outputDir=assert(p.outputDir)
local runtimeDir=assert(p.runtimeDir)
local pokerPreview=assert(p.pokerPreview)
local tableBasePath=assert(p.tableBase)

local T=Color{r=0,g=0,b=0,a=0}
local ink=Color{r=7,g=5,b=3,a=255}
local table=Color{r=9,g=28,b=20,a=255}
local brass=Color{r=211,g=147,b=44,a=255}

local function load(path)
  local s=app.open(path); assert(s,"cannot open "..path); local i=Image(s.cels[1].image); s:close(); return i
end
local function alpha(pixel) return app.pixelColor.rgbaA(pixel) end
local function tightSegment(src,sx,sw)
  local x0=sx+sw-1; local y0=src.height-1; local x1=sx; local y1=0; local found=false
  for y=0,src.height-1 do for x=sx,math.min(src.width-1,sx+sw-1) do
    if alpha(src:getPixel(x,y))>8 then found=true; x0=math.min(x0,x); y0=math.min(y0,y); x1=math.max(x1,x); y1=math.max(y1,y) end
  end end
  assert(found,"empty segment")
  local out=Image(x1-x0+1,y1-y0+1,ColorMode.RGB); out:clear(T)
  for y=y0,y1 do for x=x0,x1 do out:drawPixel(x-x0,y-y0,src:getPixel(x,y)) end end
  return out
end
local function resizeNearest(src,w,h)
  local dst=Image(w,h,ColorMode.RGB); dst:clear(T)
  for y=0,h-1 do local sy=math.min(src.height-1,math.floor(y*src.height/h))
    for x=0,w-1 do local sx=math.min(src.width-1,math.floor(x*src.width/w)); dst:drawPixel(x,y,src:getPixel(sx,sy)) end
  end
  return dst
end
local function fit(src,w,h,pad)
  pad=pad or 5
  local scale=math.min((w-pad*2)/src.width,(h-pad*2)/src.height)
  local nw=math.max(1,math.floor(src.width*scale+0.5)); local nh=math.max(1,math.floor(src.height*scale+0.5))
  local out=Image(w,h,ColorMode.RGB); out:clear(T)
  out:drawImage(resizeNearest(src,nw,nh),Point(math.floor((w-nw)/2),h-pad-nh))
  return out
end
local function setImage(spr,img) spr.cels[1].image:clear(T); spr.cels[1].image:drawImage(img,Point(0,0)) end
local function save(img,path) local s=Sprite(img.width,img.height,ColorMode.RGB); setImage(s,img); s:saveAs(path); s:close() end
local function fill(img,x,y,w,h,c)
  for yy=math.max(0,y),math.min(img.height-1,y+h-1) do for xx=math.max(0,x),math.min(img.width-1,x+w-1) do img:drawPixel(xx,yy,c) end end
end
local function copyRect(src,dst,x,y,w,h)
  for yy=y,math.min(src.height-1,y+h-1) do for xx=x,math.min(src.width-1,x+w-1) do dst:drawPixel(xx,yy,src:getPixel(xx,yy)) end end
end

local src=load(sourceImage)
local states={}; local hi={}
local names={"closed","open_empty","open_filled"}
for i=0,2 do
  local sx=math.floor(src.width*i/3); local ex=math.floor(src.width*(i+1)/3)-1
  hi[i+1]=tightSegment(src,sx,ex-sx+1)
  states[i+1]=fit(hi[i+1],160,160,5)
  save(hi[i+1],sourceDir.."/poker_item_crate_"..names[i+1].."_hires_0_3_4.png")
  save(states[i+1],runtimeDir.."/poker_item_crate_"..names[i+1].."_160x160_0_3_4.png")
  save(states[i+1],outputDir.."/poker_item_crate_"..names[i+1].."_160x160_0_3_4.png")
end

local spr=Sprite(160,160,ColorMode.RGB); spr.layers[1].name="western_crate_closed_empty_filled"
setImage(spr,states[1]); spr.frames[1].duration=0.25
for i=2,3 do local f=spr:newEmptyFrame(); spr:newCel(spr.layers[1],f,states[i],Point(0,0)); f.duration=0.25 end
spr:saveAs(sourceDir.."/poker_item_crate_states_0_3_4.aseprite"); spr:close()

local sheet=Image(480,160,ColorMode.RGB); sheet:clear(T)
for i=1,3 do sheet:drawImage(states[i],Point((i-1)*160,0)) end
save(sheet,previewDir.."/poker_item_crate_states_480x160_0_3_4.png")
save(sheet,outputDir.."/poker_item_crate_states_480x160_0_3_4.png")

local base=load(pokerPreview); local tableBase=load(tableBasePath)
-- Cover the earlier flat placeholder crates and place the richer western props.
copyRect(tableBase,base,610,78,190,180); copyRect(tableBase,base,610,314,190,185)
base:drawImage(states[1],Point(625,86)); base:drawImage(states[3],Point(625,326))
save(base,previewDir.."/poker_item_crate_application_preview_960x540_0_3_4.png")
save(base,outputDir.."/poker_item_crate_application_preview_960x540_0_3_4.png")

local compare=Image(960,360,ColorMode.RGB); compare:clear(ink)
local old=load(assert(p.oldSheet)); local oldScale=resizeNearest(old,480,137)
compare:drawImage(oldScale,Point(240,18)); fill(compare,0,177,960,4,brass); compare:drawImage(sheet,Point(240,190))
save(compare,previewDir.."/poker_item_crate_before_after_960x360_0_3_4.png")
save(compare,outputDir.."/poker_item_crate_before_after_960x360_0_3_4.png")
print("Poker crates 0.3.4 generated")
