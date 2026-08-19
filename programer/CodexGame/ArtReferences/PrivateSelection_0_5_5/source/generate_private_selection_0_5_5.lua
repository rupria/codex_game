-- Private-card selection popup 0.5.5.
-- Produces an opaque western modal, candidate states and a 960x540 review preview.

local p=app.params
local outDir=assert(p.outDir)
local previewDir=assert(p.previewDir)
local sourceDir=assert(p.sourceDir)
local backgroundPath=assert(p.background)
local jokerPath=assert(p.joker)
local publicAPath=assert(p.publicA)
local publicBPath=assert(p.publicB)
local candidateAPath=assert(p.candidateA)
local candidateBPath=assert(p.candidateB)

local C={
  clear=Color{r=0,g=0,b=0,a=0}, black=Color{r=5,g=5,b=5,a=255},
  panel=Color{r=14,g=13,b=11,a=255}, panel2=Color{r=21,g=18,b=14,a=255},
  content=Color{r=9,g=18,b=18,a=255}, leather=Color{r=48,g=28,b=18,a=255},
  brassDark=Color{r=83,g=48,b=15,a=255}, brass=Color{r=174,g=103,b=24,a=255},
  brassHi=Color{r=244,g=185,b=62,a=255}, tealDark=Color{r=10,g=68,b=73,a=255},
  teal=Color{r=31,g=201,b=210,a=255}, tealHi=Color{r=116,g=246,b=239,a=255},
  iron=Color{r=71,g=74,b=74,a=255}, cream=Color{r=236,g=220,b=181,a=255},
  red=Color{r=153,g=43,b=35,a=255}
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
local function frame(img,x,y,w,h,edge,inner)
  fill(img,x,y,w,h,C.black); fill(img,x+2,y+2,w-4,h-4,C.brassDark)
  fill(img,x+4,y+4,w-8,h-8,inner or C.panel2)
  hline(img,x+12,x+w-13,y+5,edge); hline(img,x+12,x+w-13,y+h-6,C.brassDark)
  vline(img,x+5,y+12,y+h-13,edge); vline(img,x+w-6,y+12,y+h-13,C.brassDark)
  diamond(img,x+7,y+7,3,edge); diamond(img,x+w-8,y+7,3,C.brass)
  diamond(img,x+7,y+h-8,3,C.brass); diamond(img,x+w-8,y+h-8,3,edge)
end

local dim=Image(960,540,ColorMode.RGB); dim:clear(Color{r=0,g=0,b=0,a=176})
save(dim,outDir..'/private_selection_modal_dim_960x540_0_5_5.png')

local panel=Image(860,456,ColorMode.RGB); panel:clear(C.clear)
frame(panel,0,0,860,456,C.brassHi,C.panel)
frame(panel,18,16,824,54,C.brass,C.panel2)
-- Public/community reference column.
frame(panel,18,82,190,226,C.brass,C.content)
-- Candidate area stays opaque and is visibly separated from the table.
frame(panel,220,82,622,336,C.teal,C.content)
-- Required/selected count chip and confirm safe frame.
frame(panel,18,320,190,42,C.brass,C.panel2)
diamond(panel,430,42,6,C.brassHi); diamond(panel,430,42,3,C.leather)
save(panel,outDir..'/private_selection_modal_panel_860x456_0_5_5.png')
save(panel,sourceDir..'/private_selection_modal_panel_0_5_5.aseprite')

local publicFrame=Image(166,198,ColorMode.RGB); publicFrame:clear(C.clear)
frame(publicFrame,0,0,166,198,C.brass,C.content)
hline(publicFrame,18,147,35,C.brassDark)
save(publicFrame,outDir..'/private_selection_public_frame_166x198_0_5_5.png')

local function candidateState(name,edge,inner,mark)
  local img=Image(112,150,ColorMode.RGB); img:clear(C.clear)
  frame(img,0,0,112,150,edge,inner)
  fill(img,11,13,90,120,Color{r=4,g=7,b=7,a=180})
  if mark then
    diamond(img,56,139,7,edge); diamond(img,56,139,3,C.black)
  else
    hline(img,42,70,140,C.brassDark)
  end
  save(img,outDir..'/private_selection_candidate_'..name..'_112x150_0_5_5.png')
  save(img,sourceDir..'/private_selection_candidate_'..name..'_0_5_5.aseprite')
  return img
end

local idle=candidateState('idle',C.brassDark,C.panel2,false)
local hover=candidateState('hover',C.teal,C.panel2,true)
local selected=candidateState('selected',C.tealHi,C.tealDark,true)
local confirmed=candidateState('confirmed',C.brassHi,C.leather,true)
local disabled=candidateState('disabled',C.iron,C.panel,false)

local function button(name,edge,inner)
  local img=Image(180,52,ColorMode.RGB); img:clear(C.clear)
  frame(img,0,0,180,52,edge,inner)
  hline(img,42,138,26,edge)
  save(img,outDir..'/private_selection_confirm_'..name..'_180x52_0_5_5.png')
  return img
end
local confirmIdle=button('idle',C.brass,C.panel2)
local confirmActive=button('active',C.tealHi,C.tealDark)
local confirmDisabled=button('disabled',C.iron,C.panel)

local background=load(backgroundPath)
if background.width~=960 or background.height~=540 then background=resize(background,960,540) end
local preview=Image(background)
-- Preview uses an opaque darkening pass so the modal contrast is judged accurately.
for y=0,539 do for x=0,959 do
  local px=preview:getPixel(x,y); local r=app.pixelColor.rgbaR(px); local g=app.pixelColor.rgbaG(px); local b=app.pixelColor.rgbaB(px)
  preview:drawPixel(x,y,Color{r=math.floor(r*0.38),g=math.floor(g*0.38),b=math.floor(b*0.38),a=255})
end end
preview:drawImage(panel,Point(50,42))

local pubA=resize(load(publicAPath),56,78); local pubB=resize(load(publicBPath),56,78)
preview:drawImage(pubA,Point(88,158)); preview:drawImage(pubB,Point(148,158))
-- Public-card text-safe placeholder and required-count indicator.
fill(preview,92,138,108,2,C.brass); fill(preview,92,272,108,2,C.brassDark)
fill(preview,92,384,108,2,C.cream)
preview:drawImage(confirmActive,Point(73,418))

local cardA=resize(load(candidateAPath),84,117)
local cardB=resize(load(candidateBPath),84,117)
local joker=resize(load(jokerPath),84,117)
local frames={idle,selected,hover,confirmed,disabled,idle}
local cards={cardA,joker,cardB,cardA,cardB,joker}
local pos={{292,136},{424,136},{556,136},{688,136},{358,296},{490,296}}
for i=1,#pos do
  preview:drawImage(frames[i],Point(pos[i][1],pos[i][2]))
  preview:drawImage(cards[i],Point(pos[i][1]+14,pos[i][2]+13))
end
-- Localized title/guide placeholders only; no words are baked into runtime art.
fill(preview,324,74,312,2,C.brassHi); fill(preview,364,96,232,2,C.brassDark)
save(preview,previewDir..'/private_selection_joker_popup_preview_960x540_0_5_5.png')

print('PrivateSelection 0.5.5 review art generated')
