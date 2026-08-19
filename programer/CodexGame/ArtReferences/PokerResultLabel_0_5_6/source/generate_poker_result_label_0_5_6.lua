-- Poker result message panel 0.5.6 for GitHub issue #58.
-- Three heights and three accent states keep localized/item result messages readable.

local p=app.params
local outDir=assert(p.outDir)
local previewDir=assert(p.previewDir)
local sourceDir=assert(p.sourceDir)
local screenshotPath=assert(p.screenshot)

local C={
  clear=Color{r=0,g=0,b=0,a=0}, black=Color{r=5,g=5,b=5,a=255},
  panel=Color{r=15,g=12,b=10,a=255}, panel2=Color{r=23,g=18,b=14,a=255},
  leather=Color{r=46,g=27,b=17,a=255}, brassDark=Color{r=82,g=47,b=16,a=255},
  brass=Color{r=173,g=103,b=27,a=255}, brassHi=Color{r=241,g=181,b=63,a=255},
  teal=Color{r=40,g=211,b=202,a=255}, red=Color{r=231,g=73,b=67,a=255},
  cream=Color{r=238,g=222,b=187,a=255}, iron=Color{r=93,g=92,b=86,a=255}
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

local function panel(height,accent,state,heightName)
  local img=Image(788,height,ColorMode.RGB); img:clear(C.clear)
  fill(img,0,0,788,height,C.black)
  fill(img,2,2,784,height-4,C.brassDark)
  fill(img,4,4,780,height-8,C.panel)
  fill(img,10,10,768,height-20,C.panel2)
  -- Thin outcome accent preserves message space and replaces the oversized red bars.
  fill(img,4,4,780,3,accent); fill(img,4,height-7,780,3,accent)
  vline(img,9,16,height-17,C.brass); vline(img,778,16,height-17,C.brassDark)
  diamond(img,12,12,4,accent); diamond(img,775,12,4,C.brass)
  diamond(img,12,height-13,4,C.brass); diamond(img,775,height-13,4,accent)
  -- Reserved result badge slot; text starts after x=64.
  fill(img,18,math.floor(height/2)-22,44,44,C.black)
  fill(img,20,math.floor(height/2)-20,40,40,C.leather)
  diamond(img,40,math.floor(height/2),8,accent)
  diamond(img,40,math.floor(height/2),3,C.panel)
  save(img,outDir..'/poker_result_message_panel_'..state..'_'..heightName..'_788x'..height..'_0_5_6.png')
  return img
end

local heights={{108,'compact'},{132,'standard'},{164,'expanded'}}
local states={{'success',C.teal},{'failure',C.red},{'neutral',C.brassHi}}
local generated={}
for _,s in ipairs(states) do
  for _,h in ipairs(heights) do
    local key=s[1]..'_'..h[2]
    generated[key]=panel(h[1],s[2],s[1],h[2])
  end
end
save(generated.neutral_standard,sourceDir..'/poker_result_message_panel_0_5_6.aseprite')

local chip=Image(360,32,ColorMode.RGB); chip:clear(C.clear)
fill(chip,0,0,360,32,C.black); fill(chip,2,2,356,28,C.brassDark); fill(chip,4,4,352,24,C.leather)
hline(chip,16,343,5,C.brass); hline(chip,16,343,26,C.brassDark)
diamond(chip,10,16,4,C.brassHi); diamond(chip,349,16,4,C.brass)
save(chip,outDir..'/poker_result_item_status_chip_360x32_0_5_6.png')

local shot=resize(load(screenshotPath),960,540)
local preview=Image(shot)
-- Cover the old fixed 92px label with the new opaque standard panel.
preview:drawImage(generated.failure_standard,Point(86,178))
-- Text-safe placeholders: summary stays two lines, optional item status is separate.
fill(preview,176,204,598,2,C.cream); fill(preview,214,232,522,2,C.cream)
preview:drawImage(chip,Point(300,264)); fill(preview,356,279,248,2,C.red)
save(preview,previewDir..'/issue_58_result_label_standard_preview_960x540_0_5_6.png')

local expanded=Image(shot)
expanded:drawImage(generated.success_expanded,Point(86,160))
fill(expanded,176,185,598,2,C.cream); fill(expanded,214,213,522,2,C.cream)
fill(expanded,232,241,486,2,C.cream); expanded:drawImage(chip,Point(300,276)); fill(expanded,356,291,248,2,C.teal)
save(expanded,previewDir..'/issue_58_result_label_expanded_preview_960x540_0_5_6.png')

local sheet=Image(820,440,ColorMode.RGB); sheet:clear(Color{r=8,g=7,b=6,a=255})
sheet:drawImage(generated.neutral_compact,Point(16,16))
sheet:drawImage(generated.failure_standard,Point(16,140))
sheet:drawImage(generated.success_expanded,Point(16,280))
save(sheet,previewDir..'/poker_result_label_height_states_820x440_0_5_6.png')

print('PokerResultLabel 0.5.6 review art generated')
