-- GitHub issues #48, #49 and #51 art package 0.5.4.
-- Produces transient last-five badges, rope-end VFX and a bounded reward panel.

local p=app.params
local halliRuntimeDir=assert(p.halliRuntimeDir)
local rewardRuntimeDir=assert(p.rewardRuntimeDir)
local halliPreviewDir=assert(p.halliPreviewDir)
local rewardPreviewDir=assert(p.rewardPreviewDir)
local halliSourceDir=assert(p.halliSourceDir)
local rewardSourceDir=assert(p.rewardSourceDir)
local backgroundPath=assert(p.background)

local C={
  clear=Color{r=0,g=0,b=0,a=0}, ink=Color{r=7,g=6,b=5,a=255},
  panel=Color{r=18,g=14,b=11,a=244}, panel2=Color{r=30,g=22,b=16,a=250},
  leather=Color{r=74,g=37,b=19,a=255}, leatherHi=Color{r=125,g=67,b=31,a=255},
  brassDark=Color{r=82,g=48,b=16,a=255}, brass=Color{r=181,g=108,b=27,a=255},
  brassHi=Color{r=249,g=192,b=65,a=255}, cream=Color{r=255,g=236,b=180,a=255},
  ember=Color{r=255,g=75,b=8,a=255}, orange=Color{r=255,g=132,b=10,a=255},
  yellow=Color{r=255,g=224,b=80,a=255}, white=Color{r=255,g=251,b=222,a=255},
  smoke=Color{r=74,g=64,b=57,a=210}, smokeDark=Color{r=38,g=34,b=32,a=220},
  teal=Color{r=41,g=206,b=207,a=255}, red=Color{r=237,g=61,b=58,a=255},
  green=Color{r=21,g=48,b=39,a=255}
}

local function load(path)
  local s=app.open(path); assert(s,'cannot open '..path)
  local i=Image(s.cels[1].image); s:close(); return i
end
local function fill(img,x,y,w,h,c)
  local x0=math.max(0,math.floor(x)); local y0=math.max(0,math.floor(y))
  local x1=math.min(img.width-1,math.floor(x+w-1)); local y1=math.min(img.height-1,math.floor(y+h-1))
  for yy=y0,y1 do for xx=x0,x1 do img:drawPixel(xx,yy,c) end end
end
local function hline(img,x0,x1,y,c) fill(img,x0,y,x1-x0+1,1,c) end
local function vline(img,x,y0,y1,c) fill(img,x,y0,1,y1-y0+1,c) end
local function line(img,x0,y0,x1,y1,c)
  x0=math.floor(x0); y0=math.floor(y0); x1=math.floor(x1); y1=math.floor(y1)
  local dx=math.abs(x1-x0); local sx=x0<x1 and 1 or -1
  local dy=-math.abs(y1-y0); local sy=y0<y1 and 1 or -1; local err=dx+dy
  while true do
    if x0>=0 and y0>=0 and x0<img.width and y0<img.height then img:drawPixel(x0,y0,c) end
    if x0==x1 and y0==y1 then break end
    local e2=2*err
    if e2>=dy then err=err+dy; x0=x0+sx end
    if e2<=dx then err=err+dx; y0=y0+sy end
  end
end
local function disk(img,cx,cy,r,c)
  for yy=-r,r do local span=math.floor(math.sqrt(math.max(0,r*r-yy*yy))); hline(img,cx-span,cx+span,cy+yy,c) end
end
local function diamond(img,cx,cy,r,c)
  for yy=-r,r do local span=r-math.abs(yy); hline(img,cx-span,cx+span,cy+yy,c) end
end
local function star(img,cx,cy,r,c)
  diamond(img,cx,cy,r,c); hline(img,cx-r*2,cx+r*2,cy,c); vline(img,cx,cy-r*2,cy+r*2,c)
  line(img,cx-r,cy-r,cx+r,cy+r,c); line(img,cx-r,cy+r,cx+r,cy-r,c)
end
local function resize(src,w,h)
  local dst=Image(w,h,ColorMode.RGB); dst:clear(C.clear)
  for y=0,h-1 do local sy=math.min(src.height-1,math.floor(y*src.height/h))
    for x=0,w-1 do local sx=math.min(src.width-1,math.floor(x*src.width/w)); dst:drawPixel(x,y,src:getPixel(sx,sy)) end
  end
  return dst
end
local function save(img,path)
  local s=Sprite(img.width,img.height,ColorMode.RGB); s.cels[1].image:clear(C.clear)
  s.cels[1].image:drawImage(img,Point(0,0)); s:saveAs(path); s:close()
end
local function panelBorder(img,x,y,w,h,active)
  local hi=active and C.brassHi or C.brass
  fill(img,x,y,w,h,C.ink); fill(img,x+2,y+2,w-4,h-4,C.brassDark)
  fill(img,x+5,y+5,w-10,h-10,C.panel)
  hline(img,x+13,x+w-14,y+5,hi); hline(img,x+13,x+w-14,y+h-6,C.brass)
  vline(img,x+5,y+13,y+h-14,hi); vline(img,x+w-6,y+13,y+h-14,C.brass)
  diamond(img,x+7,y+7,3,C.brassHi); diamond(img,x+w-8,y+7,3,C.brassHi)
  diamond(img,x+7,y+h-8,3,C.brass); diamond(img,x+w-8,y+h-8,3,C.brass)
end

local digits={
  ['1']={'01100','11100','01100','01100','01100','01100','11111'},
  ['2']={'11110','00011','00011','01110','11000','11000','11111'},
  ['3']={'11110','00011','00011','01110','00011','00011','11110'},
  ['4']={'11000','11010','11010','11111','00010','00010','00010'},
  ['5']={'11111','11000','11000','11110','00011','00011','11110'}
}
local function pixelDigit(img,d,cx,cy,scale,color)
  local map=digits[tostring(d)]
  local ox=math.floor(cx-(5*scale)/2); local oy=math.floor(cy-(7*scale)/2)
  for row=1,7 do for col=1,5 do if map[row]:sub(col,col)=='1' then fill(img,ox+(col-1)*scale,oy+(row-1)*scale,scale,scale,color) end end end
end

local function countdownBadge(d)
  local img=Image(96,96,ColorMode.RGB); img:clear(C.clear)
  disk(img,48,48,43,C.ink); disk(img,48,48,40,C.brassDark); disk(img,48,48,35,C.leather)
  disk(img,48,48,30,C.panel2); hline(img,26,70,19,C.brassHi); hline(img,26,70,77,C.brass)
  diamond(img,48,9,5,C.brassHi); diamond(img,48,87,5,C.brass); diamond(img,9,48,5,C.brass); diamond(img,87,48,5,C.brass)
  pixelDigit(img,d,48,48,7,C.cream); star(img,74,22,2,C.brassHi)
  return img
end

local countdownFrames={}; local countdownSheet=Image(480,96,ColorMode.RGB); countdownSheet:clear(C.clear)
for i=1,5 do local d=6-i; local fr=countdownBadge(d); countdownFrames[i]=fr; countdownSheet:drawImage(fr,Point((i-1)*96,0)) end
save(countdownSheet,halliRuntimeDir..'/halli_last_five_countdown_sheet_5f_480x96_0_5_4.png')
local plate=countdownBadge(5); fill(plate,31,27,34,43,C.panel2)
save(plate,halliRuntimeDir..'/halli_last_five_countdown_plate_96_0_5_4.png')

local countdownSource=Sprite(96,96,ColorMode.RGB); countdownSource.layers[1].name='countdown_5_to_1'
countdownSource.cels[1].image:clear(C.clear); countdownSource.cels[1].image:drawImage(countdownFrames[1],Point(0,0)); countdownSource.frames[1].duration=0.48
for i=2,5 do local f=countdownSource:newEmptyFrame(); countdownSource:newCel(countdownSource.layers[1],f,countdownFrames[i],Point(0,0)); f.duration=0.48 end
countdownSource:saveAs(halliSourceDir..'/halli_last_five_countdown_0_5_4.aseprite'); countdownSource:close()

local function flameFrame(i)
  local fr=Image(48,48,ColorMode.RGB); fr:clear(C.clear)
  local ax=14; local ay=24
  local sway=(i%3)-1
  star(fr,ax,ay,2,C.yellow); disk(fr,ax+3,ay-1,3,C.ember)
  -- The blaze grows into the already-burned area to the right of the rope end.
  diamond(fr,ax+7+sway,ay-7-(i%2)*2,7,C.ember)
  diamond(fr,ax+8-sway,ay-10-(i%3),5,C.orange)
  diamond(fr,ax+6+sway,ay-7,3,C.yellow)
  diamond(fr,ax+10-sway,ay-16-(i%2)*2,3,C.ember)
  diamond(fr,ax+13+(i%2)*3,ay+4+(i%3),2,C.orange)
  diamond(fr,ax+19-(i%3)*2,ay-16-(i%2)*3,3,C.smoke)
  diamond(fr,ax+23+(i%2)*2,ay-24+(i%3),4,C.smokeDark)
  return fr
end
local flameFrames={}; local flameSheet=Image(288,48,ColorMode.RGB); flameSheet:clear(C.clear)
for i=1,6 do local fr=flameFrame(i); flameFrames[i]=fr; flameSheet:drawImage(fr,Point((i-1)*48,0)) end
save(flameSheet,halliRuntimeDir..'/halli_rope_contact_flame_6f_288x48_0_5_4.png')

local function burstFrame(i)
  local fr=Image(96,96,ColorMode.RGB); fr:clear(C.clear); local ax=24; local ay=48
  if i<=6 then
    local r=2+i*4; star(fr,ax,ay,r,(i<=2) and C.white or C.brassHi)
    diamond(fr,ax,ay,math.max(2,10-i),i<=3 and C.yellow or C.ember)
    local pieces=3+i*2
    for n=1,pieces do
      local ang=(n*2.12+i*.57); local rr=9+i*4+((n*7)%8)
      local x=math.floor(ax+math.cos(ang)*rr); local y=math.floor(ay+math.sin(ang)*rr)
      diamond(fr,x,y,(n%4==0) and 3 or 2,(n%2==0) and C.orange or C.smokeDark)
    end
  end
  if i>=4 then
    local alpha=math.max(35,225-i*24)
    local smoke=Color{r=55,g=48,b=43,a=alpha}
    disk(fr,ax+8+i*3,ay-10-(i%2)*5,5+i,smoke)
    disk(fr,ax+20+i*2,ay+4+(i%3)*4,4+i,C.smokeDark)
  end
  return fr
end
local burstFrames={}; local burstSheet=Image(768,96,ColorMode.RGB); burstSheet:clear(C.clear)
for i=1,8 do local fr=burstFrame(i); burstFrames[i]=fr; burstSheet:drawImage(fr,Point((i-1)*96,0)) end
save(burstSheet,halliRuntimeDir..'/halli_rope_terminal_burst_8f_768x96_0_5_4.png')
local scorch=Image(32,24,ColorMode.RGB); scorch:clear(C.clear); disk(scorch,10,12,8,Color{r=17,g=13,b=10,a=190}); star(scorch,10,12,4,C.smokeDark)
save(scorch,halliRuntimeDir..'/halli_rope_terminal_scorch_32x24_0_5_4.png')

local flameSource=Sprite(48,48,ColorMode.RGB); flameSource.layers[1].name='contact_anchor_14_24'
flameSource.cels[1].image:drawImage(flameFrames[1],Point(0,0)); flameSource.frames[1].duration=0.08
for i=2,6 do local f=flameSource:newEmptyFrame(); flameSource:newCel(flameSource.layers[1],f,flameFrames[i],Point(0,0)); f.duration=0.08 end
flameSource:saveAs(halliSourceDir..'/halli_rope_contact_flame_0_5_4.aseprite'); flameSource:close()
local burstSource=Sprite(96,96,ColorMode.RGB); burstSource.layers[1].name='terminal_anchor_24_48'
burstSource.cels[1].image:drawImage(burstFrames[1],Point(0,0)); burstSource.frames[1].duration=0.05
for i=2,8 do local f=burstSource:newEmptyFrame(); burstSource:newCel(burstSource.layers[1],f,burstFrames[i],Point(0,0)); f.duration=(i>=6) and 0.12 or 0.06 end
burstSource:saveAs(halliSourceDir..'/halli_rope_terminal_burst_0_5_4.aseprite'); burstSource:close()

local rewardPanel=Image(720,300,ColorMode.RGB); rewardPanel:clear(C.clear); panelBorder(rewardPanel,0,0,720,300,true)
fill(rewardPanel,24,58,672,198,Color{r=8,g=11,b=10,a=210}); hline(rewardPanel,32,688,58,C.brassDark)
diamond(rewardPanel,360,18,7,C.brassHi); hline(rewardPanel,112,608,32,C.brass); hline(rewardPanel,112,608,47,C.brassDark)
save(rewardPanel,rewardRuntimeDir..'/stage_reward_summary_panel_720x300_0_5_4.png')
local rewardRow=Image(320,64,ColorMode.RGB); rewardRow:clear(C.clear); panelBorder(rewardRow,0,0,320,64,false)
fill(rewardRow,16,14,36,36,C.panel2); diamond(rewardRow,34,32,8,C.brassHi); hline(rewardRow,68,294,23,C.brassDark); hline(rewardRow,68,260,42,C.brass)
save(rewardRow,rewardRuntimeDir..'/stage_reward_row_frame_320x64_0_5_4.png')
local overflow=Image(640,32,ColorMode.RGB); overflow:clear(C.clear)
for y=0,31 do fill(overflow,0,y,640,1,Color{r=8,g=11,b=10,a=math.floor(y/31*230)}) end
diamond(overflow,320,26,4,C.brassHi)
save(overflow,rewardRuntimeDir..'/stage_reward_overflow_fade_640x32_0_5_4.png')
local rewardSource=Sprite(720,300,ColorMode.RGB); rewardSource.layers[1].name='safe_content_24_58_672_198'; rewardSource.cels[1].image:drawImage(rewardPanel,Point(0,0))
rewardSource:saveAs(rewardSourceDir..'/stage_reward_summary_panel_0_5_4.aseprite'); rewardSource:close()

local bg=load(backgroundPath); if bg.width~=960 or bg.height~=540 then bg=resize(bg,960,540) end
local function dimBackground()
  local img=Image(bg); fill(img,0,0,960,540,Color{r=2,g=3,b=3,a=72}); return img
end

local halliPreview=dimBackground()
fill(halliPreview,174,126,612,270,Color{r=5,g=12,b=10,a=165})
-- Rope with an exact terminal point at (332,152), effect grows into burned/right area.
for x=332,589 do
  local y=150+((x%10)<5 and 0 or 2); fill(halliPreview,x,y,2,5,(x%12<6) and C.leatherHi or C.brass)
end
halliPreview:drawImage(flameFrames[3],Point(332-14,152-24))
-- Last-five badge is transient and does not replace any persistent top label.
halliPreview:drawImage(countdownFrames[1],Point(432,218))
-- Minimal card silhouettes and bells to prove collision-free safe area.
panelBorder(halliPreview,210,238,90,126,false); panelBorder(halliPreview,660,238,90,126,false)
disk(halliPreview,386,374,20,C.brassDark); disk(halliPreview,386,374,15,C.brassHi)
disk(halliPreview,574,374,20,C.brassDark); disk(halliPreview,574,374,15,C.brassHi)
save(halliPreview,halliPreviewDir..'/issue_48_51_halli_application_preview_960x540_0_5_4.png')

local halliBoard=Image(1920,540,ColorMode.RGB); halliBoard:clear(C.ink)
local left=dimBackground(); left:drawImage(countdownFrames[5],Point(432,220)); panelBorder(left,210,238,90,126,false); panelBorder(left,660,238,90,126,false)
local right=dimBackground(); right:drawImage(burstFrames[4],Point(332-24,152-48)); right:drawImage(scorch,Point(332-10,152-12))
halliBoard:drawImage(left,Point(0,0)); halliBoard:drawImage(right,Point(960,0))
save(halliBoard,halliPreviewDir..'/issue_48_51_halli_storyboard_1920x540_0_5_4.png')

local rewardPreview=dimBackground(); fill(rewardPreview,0,0,960,540,Color{r=1,g=2,b=2,a=105}); rewardPreview:drawImage(rewardPanel,Point(120,102))
rewardPreview:drawImage(rewardRow,Point(144,174)); rewardPreview:drawImage(rewardRow,Point(496,174))
rewardPreview:drawImage(rewardRow,Point(144,246)); rewardPreview:drawImage(rewardRow,Point(496,246))
rewardPreview:drawImage(overflow,Point(160,324)); panelBorder(rewardPreview,360,430,240,54,true)
save(rewardPreview,rewardPreviewDir..'/issue_49_stage_reward_safe_layout_preview_960x540_0_5_4.png')

print('Issue 48, 49 and 51 art 0.5.4 generated')
