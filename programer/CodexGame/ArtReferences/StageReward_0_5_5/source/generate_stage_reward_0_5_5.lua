-- Stage reward popup 0.5.5 review package for GitHub issue #49.
-- Creates an opaque, centered panel that contains rows and the Continue button.

local p=app.params
local outDir=assert(p.outDir)
local previewDir=assert(p.previewDir)
local sourceDir=assert(p.sourceDir)
local backgroundPath=assert(p.background)
local baseIconPath=assert(p.baseIcon)
local tempIconPath=assert(p.tempIcon)

local C={
  clear=Color{r=0,g=0,b=0,a=0}, black=Color{r=6,g=6,b=6,a=255},
  panel=Color{r=15,g=14,b=12,a=255}, panel2=Color{r=20,g=18,b=15,a=255},
  content=Color{r=11,g=14,b=13,a=255}, leather=Color{r=42,g=25,b=17,a=255},
  brassDark=Color{r=88,g=52,b=18,a=255}, brass=Color{r=177,g=106,b=28,a=255},
  brassHi=Color{r=244,g=183,b=65,a=255}, teal=Color{r=30,g=154,b=163,a=255},
  cream=Color{r=234,g=219,b=181,a=255}
}

local function load(path)
  local s=app.open(path); assert(s,'cannot open '..path)
  local i=Image(s.cels[1].image); s:close(); return i
end
local function save(img,path)
  local s=Sprite(img.width,img.height,ColorMode.RGB)
  s.cels[1].image:clear(C.clear); s.cels[1].image:drawImage(img,Point(0,0)); s:saveAs(path); s:close()
end
local function resize(src,w,h)
  local dst=Image(w,h,ColorMode.RGB); dst:clear(C.clear)
  for y=0,h-1 do local sy=math.min(src.height-1,math.floor(y*src.height/h))
    for x=0,w-1 do local sx=math.min(src.width-1,math.floor(x*src.width/w)); dst:drawPixel(x,y,src:getPixel(sx,sy)) end
  end
  return dst
end
local function fill(img,x,y,w,h,c)
  local x0=math.max(0,math.floor(x)); local y0=math.max(0,math.floor(y))
  local x1=math.min(img.width-1,math.floor(x+w-1)); local y1=math.min(img.height-1,math.floor(y+h-1))
  for yy=y0,y1 do for xx=x0,x1 do img:drawPixel(xx,yy,c) end end
end
local function hline(img,x0,x1,y,c) fill(img,x0,y,x1-x0+1,1,c) end
local function vline(img,x,y0,y1,c) fill(img,x,y0,1,y1-y0+1,c) end
local function diamond(img,cx,cy,r,c)
  for yy=-r,r do local s=r-math.abs(yy); hline(img,cx-s,cx+s,cy+yy,c) end
end
local function frame(img,x,y,w,h,active)
  fill(img,x,y,w,h,C.black)
  fill(img,x+2,y+2,w-4,h-4,C.brassDark)
  fill(img,x+4,y+4,w-8,h-8,C.panel2)
  local edge=active and C.brassHi or C.brass
  hline(img,x+14,x+w-15,y+5,edge); hline(img,x+14,x+w-15,y+h-6,C.brassDark)
  vline(img,x+5,y+14,y+h-15,edge); vline(img,x+w-6,y+14,y+h-15,C.brassDark)
  diamond(img,x+7,y+7,4,C.brassHi); diamond(img,x+w-8,y+7,4,C.brass)
  diamond(img,x+7,y+h-8,4,C.brass); diamond(img,x+w-8,y+h-8,4,C.brassHi)
end

local panel=Image(680,360,ColorMode.RGB); panel:clear(C.clear)
frame(panel,0,0,680,360,true)
-- Opaque inner surface prevents the previous StageClear art from showing through.
fill(panel,14,14,652,332,C.panel)
frame(panel,20,18,640,58,false)
fill(panel,42,80,596,1,C.brassDark)
fill(panel,24,90,632,154,C.content)
frame(panel,20,86,640,162,false)
-- Integrated localized Continue button safe area.
frame(panel,220,288,240,52,true)
-- Small centered western ornament, no baked text.
diamond(panel,340,30,6,C.brassHi); diamond(panel,340,30,3,C.leather)
save(panel,outDir..'/stage_reward_summary_panel_680x360_0_5_5.png')
save(panel,sourceDir..'/stage_reward_summary_panel_0_5_5.aseprite')

local row=Image(304,64,ColorMode.RGB); row:clear(C.clear)
frame(row,0,0,304,64,false)
fill(row,70,17,210,1,C.brassDark); fill(row,70,46,210,1,C.brassDark)
fill(row,68,18,214,28,C.panel)
save(row,outDir..'/stage_reward_row_frame_304x64_0_5_5.png')
save(row,sourceDir..'/stage_reward_row_frame_0_5_5.aseprite')

local contentMask=Image(632,154,ColorMode.RGB); contentMask:clear(C.clear)
fill(contentMask,0,0,632,154,C.content)
save(contentMask,outDir..'/stage_reward_content_opaque_632x154_0_5_5.png')

local background=load(backgroundPath)
if background.width~=960 or background.height~=540 then background=resize(background,960,540) end
local baseIcon=resize(load(baseIconPath),40,40)
local tempIcon=resize(load(tempIconPath),40,40)
local preview=Image(background)
-- Subtle modal dim behind the single reward popup.
fill(preview,0,0,960,540,Color{r=0,g=0,b=0,a=125})
preview:drawImage(panel,Point(140,90))
preview:drawImage(row,Point(164,184)); preview:drawImage(row,Point(492,184))
preview:drawImage(baseIcon,Point(180,196)); preview:drawImage(tempIcon,Point(508,196))
-- Text-safe placeholders demonstrate containment without baking locale strings.
fill(preview,246,202,184,2,C.brassDark); fill(preview,246,226,116,2,C.cream)
fill(preview,574,202,184,2,C.brassDark); fill(preview,574,226,116,2,C.cream)
fill(preview,304,116,352,2,C.brass); fill(preview,350,146,260,2,C.brassDark)
save(preview,previewDir..'/issue_49_stage_reward_centered_preview_960x540_0_5_5.png')

print('StageReward 0.5.5 review art generated')
