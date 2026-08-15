-- Builds the 0.1.2.4 Western bell state set from one transparent master.
-- Required params: master, runtimeRoot, presentationRoot, sourceRoot,
-- previewRoot, board, opponent

local p = app.params
local masterPath = assert(p.master, "master is required")
local runtimeRoot = assert(p.runtimeRoot, "runtimeRoot is required")
local presentationRoot = assert(p.presentationRoot, "presentationRoot is required")
local sourceRoot = assert(p.sourceRoot, "sourceRoot is required")
local previewRoot = assert(p.previewRoot, "previewRoot is required")
local boardPath = assert(p.board, "board is required")
local opponentPath = assert(p.opponent, "opponent is required")

local pc = app.pixelColor
local function clamp(v) return math.max(0, math.min(255, math.floor(v + 0.5))) end
local function rgba(r,g,b,a) return pc.rgba(clamp(r), clamp(g), clamp(b), clamp(a)) end

local function transparentImage(w,h)
  local img = Image(w,h,ColorMode.RGB)
  img:clear(Color{r=0,g=0,b=0,a=0})
  return img
end

local function transform(src, fn)
  local dst = transparentImage(src.width, src.height)
  for y=0,src.height-1 do
    for x=0,src.width-1 do
      local v=src:getPixel(x,y)
      local a=pc.rgbaA(v)
      if a>0 then
        local r,g,b=pc.rgbaR(v),pc.rgbaG(v),pc.rgbaB(v)
        local nr,ng,nb,na=fn(r,g,b,a,x,y)
        dst:drawPixel(x,y,rgba(nr,ng,nb,na or a))
      end
    end
  end
  return dst
end

local function nearest(src,w,h)
  local dst=transparentImage(w,h)
  for y=0,h-1 do
    local sy=math.min(src.height-1,math.floor(y*src.height/h))
    for x=0,w-1 do
      local sx=math.min(src.width-1,math.floor(x*src.width/w))
      dst:drawPixel(x,y,src:getPixel(sx,sy))
    end
  end
  return dst
end

local function cropImage(src,x,y,w,h)
  local dst=transparentImage(w,h)
  for yy=0,h-1 do
    for xx=0,w-1 do dst:drawPixel(xx,yy,src:getPixel(x+xx,y+yy)) end
  end
  return dst
end

local function outline(src,c)
  local dst=transparentImage(src.width,src.height)
  for y=1,src.height-2 do
    for x=1,src.width-2 do
      if pc.rgbaA(src:getPixel(x,y))==0 then
        local hit=false
        for oy=-1,1 do
          for ox=-1,1 do
            if pc.rgbaA(src:getPixel(x+ox,y+oy))>48 then hit=true end
          end
        end
        if hit then dst:drawPixel(x,y,c) end
      end
    end
  end
  dst:drawImage(src,Point(0,0))
  return dst
end

local function fill(img,x,y,w,h,c)
  for yy=y,y+h-1 do for xx=x,x+w-1 do img:drawPixel(xx,yy,c) end end
end

local function hline(img,x,y,w,c,t)
  fill(img,x,y,w,t or 1,c)
end

local function vline(img,x,y,h,c,t)
  fill(img,x,y,t or 1,h,c)
end

local function frame(img,x,y,w,h,c,t)
  hline(img,x,y,w,c,t)
  hline(img,x,y+h-(t or 1),w,c,t)
  vline(img,x,y,h,c,t)
  vline(img,x+w-(t or 1),y,h,c,t)
end

local function disc(img,cx,cy,r,c)
  for y=-r,r do
    local span=math.floor(math.sqrt(r*r-y*y))
    hline(img,cx-span,cy+y,span*2+1,c,1)
  end
end

local function glint(img,x,y,c)
  fill(img,x-1,y-6,3,13,c)
  fill(img,x-6,y-1,13,3,c)
  fill(img,x-3,y-3,7,7,c)
end

local master=assert(app.open(masterPath),"cannot open "..masterPath)
master:resize{width=256,height=256,method="bilinear"}
master:saveAs(sourceRoot.."/bell_western_master_256_0_1_2_4.aseprite")
master:resize{width=112,height=112,method="bilinear"}

local base=transparentImage(128,128)
base:drawImage(master.cels[1].image,Point(8,8))
master:close()

-- The generated master has generous chroma-removal padding. Reframe the opaque
-- bell so it occupies the existing 64x64 Unity visual rect without changing it.
local compact=cropImage(base,21,16,88,96)
local fitted=nearest(compact,106,116)
base=transparentImage(128,128)
base:drawImage(fitted,Point(11,6))

local idle=transform(base,function(r,g,b,a)
  return r*0.92,g*0.92,b*0.92,a
end)

local hover=transform(base,function(r,g,b,a)
  return r*1.12+8,g*1.09+5,b*1.02,a
end)
hover=outline(hover,Color{r=224,g=170,b=68,a=210})
glint(hover,43,49,Color{r=255,g=241,b=180,a=245})

local pressedScaled=nearest(base,118,106)
local pressed=transparentImage(128,128)
pressed:drawImage(pressedScaled,Point(5,17))
hline(pressed,24,121,80,Color{r=6,g=5,b=4,a=180},2)

local wrong=transform(base,function(r,g,b,a)
  return r*0.78+65,g*0.52,b*0.48,a
end)
local red=Color{r=224,g=48,b=40,a=245}
fill(wrong,79,39,3,12,red); fill(wrong,76,48,6,3,red)
fill(wrong,73,50,3,9,red); fill(wrong,68,57,7,3,red)
fill(wrong,66,59,3,10,red)
fill(wrong,22,91,7,3,red); fill(wrong,99,91,7,3,red)

local correct=transform(base,function(r,g,b,a)
  return r*1.15+18,g*1.10+10,b*0.88,a
end)
correct=outline(correct,Color{r=71,g=231,b=204,a=230})
glint(correct,43,49,Color{r=255,g=249,b=196,a=255})
fill(correct,18,68,8,3,Color{r=71,g=231,b=204,a=230})
fill(correct,102,68,8,3,Color{r=71,g=231,b=204,a=230})

local disabled=transform(base,function(r,g,b,a)
  local l=r*0.24+g*0.55+b*0.21
  return l*0.58,l*0.60,l*0.63,a*0.72
end)
local chain=Color{r=74,g=72,b=70,a=190}
hline(disabled,27,73,74,chain,4)
for x=31,95,16 do fill(disabled,x,70,8,10,chain) end

local names={"idle","hover","pressed","wrong","correct","disabled"}
local imgs={idle,hover,pressed,wrong,correct,disabled}
local paths={
  runtimeRoot.."/bell_idle.png",
  runtimeRoot.."/bell_hover.png",
  runtimeRoot.."/bell_pressed.png",
  runtimeRoot.."/bell_wrong.png",
  runtimeRoot.."/Halli_0_1_0/bell_correct.png",
  runtimeRoot.."/Halli_0_1_0/bell_disabled.png"
}

for i,img in ipairs(imgs) do img:saveAs(paths[i]) end

local states=Sprite(128,128,ColorMode.RGB)
states.layers[1].name="western_bell_states"
states.cels[1].image:drawImage(imgs[1],Point(0,0))
for i=2,#imgs do
  local frame=states:newEmptyFrame()
  states:newCel(states.layers[1],frame,imgs[i],Point(0,0))
end
for i,frame in ipairs(states.frames) do frame.duration=(i==3) and 0.09 or 0.18 end
states:saveAs(sourceRoot.."/bell_western_states_128_0_1_2_4.aseprite")
states:close()

local C={
  bg=Color{r=12,g=10,b=8,a=255}, panel=Color{r=27,g=21,b=15,a=255},
  frame=Color{r=126,g=78,b=25,a=255}, hi=Color{r=218,g=154,b=52,a=255},
  cyan=Color{r=55,g=219,b=210,a=255}, red=Color{r=230,g=63,b=55,a=255}
}
local sheet=Image(960,256,ColorMode.RGB); sheet:clear(C.bg)
for i,img in ipairs(imgs) do
  local x=24+(i-1)*156
  fill(sheet,x,48,132,156,C.panel)
  hline(sheet,x,48,132,C.frame,3); hline(sheet,x,201,132,C.frame,3)
  fill(sheet,x,48,3,156,C.frame); fill(sheet,x+129,48,3,156,C.frame)
  sheet:drawImage(img,Point(x+2,58))
  local pip=(i==4) and C.red or ((i==5) and C.cyan or C.hi)
  fill(sheet,x+56,216,20,4,pip)
end
sheet:saveAs(previewRoot.."/bell_ui_states_960x256_0_1_2_4.png")
local sheetSpr=Sprite(sheet.width,sheet.height,ColorMode.RGB)
sheetSpr.layers[1].name="bell_ui_state_contact_sheet"
sheetSpr.cels[1].image:drawImage(sheet,Point(0,0))
sheetSpr:saveAs(previewRoot.."/bell_ui_states_960x256_0_1_2_4.aseprite")
sheetSpr:close()

local boardSpr=assert(app.open(boardPath),"cannot open "..boardPath)
local appPreview=boardSpr.cels[1].image:clone(); boardSpr:close()
local bell64=nearest(idle,64,64)
appPreview:drawImage(bell64,Point(390,286))
appPreview:drawImage(bell64,Point(506,286))
hline(appPreview,382,356,80,C.cyan,3); fill(appPreview,378,354,5,7,C.cyan); fill(appPreview,462,354,5,7,C.cyan)
hline(appPreview,498,356,80,C.red,3); fill(appPreview,494,354,5,7,C.red); fill(appPreview,578,354,5,7,C.red)
appPreview:saveAs(previewRoot.."/bell_ui_application_preview_960x540_0_1_2_4.png")

-- Stage-entry phase seal. This replaces the former flat yellow icon while
-- preserving its exact 64x64 runtime contract and existing .meta GUID.
local seal=transparentImage(64,64)
disc(seal,32,32,31,Color{r=6,g=6,b=6,a=245})
disc(seal,32,32,28,Color{r=71,g=43,b=18,a=255})
disc(seal,32,32,25,Color{r=188,g=126,b=39,a=255})
disc(seal,32,32,22,Color{r=18,g=26,b=24,a=255})
disc(seal,32,32,19,Color{r=11,g=14,b=14,a=255})
vline(seal,7,26,13,C.cyan,3)
vline(seal,54,26,13,C.red,3)
fill(seal,11,19,3,3,C.cyan); fill(seal,50,19,3,3,C.red)
local sealBell=nearest(correct,46,46)
seal:drawImage(sealBell,Point(9,10))
glint(seal,32,7,Color{r=255,g=225,b=139,a=245})
seal:saveAs(presentationRoot.."/phase_three_call_icon_64_0_1_2_4.png")

-- Stage-entry opponent dossier. Keep the legacy 360x152 bounds and the
-- 108x108 portrait slot at (20,22), but replace the placeholder-line styling
-- with a Western leather/wood/brass presentation frame.
local dossier=transparentImage(360,152)
local leather=Color{r=19,g=14,b=11,a=246}
local leather2=Color{r=31,g=22,b=16,a=252}
local brassDark=Color{r=88,g=52,b=21,a=255}
local brass=Color{r=170,g=111,b=36,a=255}
local brassHi=Color{r=226,g=163,b=62,a=255}
local patina=Color{r=32,g=94,b=83,a=230}
fill(dossier,6,6,348,140,leather)
fill(dossier,10,10,340,132,leather2)
frame(dossier,6,6,348,140,brassDark,4)
frame(dossier,11,11,338,130,brass,2)
hline(dossier,16,16,328,Color{r=68,g=42,b=24,a=255},3)
hline(dossier,16,134,328,Color{r=8,g=7,b=6,a=255},3)

-- Clipped-corner outer brackets and rivets.
fill(dossier,0,0,28,4,brass); fill(dossier,0,0,4,28,brass)
fill(dossier,332,0,28,4,brass); fill(dossier,356,0,4,28,brass)
fill(dossier,0,148,28,4,brass); fill(dossier,0,124,4,28,brass)
fill(dossier,332,148,28,4,brass); fill(dossier,356,124,4,28,brass)
for _,pt in ipairs{{16,16},{340,16},{16,136},{340,136}} do
  disc(dossier,pt[1],pt[2],3,brassDark); disc(dossier,pt[1],pt[2],1,brassHi)
end

-- Portrait aperture, unchanged integration rect: x20 y22 w108 h108.
fill(dossier,18,20,112,112,Color{r=7,g=8,b=7,a=255})
frame(dossier,18,20,112,112,brassDark,3)
frame(dossier,21,23,106,106,Color{r=38,g=72,b=64,a=255},2)
fill(dossier,24,26,100,100,Color{r=9,g=13,b=13,a=255})

-- Sheriff-badge header and text-safe leather plaques (no baked text).
vline(dossier,140,20,112,brassDark,2)
fill(dossier,148,22,194,30,Color{r=11,g=10,b=9,a=230})
frame(dossier,148,22,194,30,brassDark,2)
fill(dossier,154,59,182,47,Color{r=14,g=12,b=10,a=210})
hline(dossier,154,59,182,patina,2)
hline(dossier,154,104,182,brassDark,2)
disc(dossier,164,37,7,brassDark)
disc(dossier,164,37,4,brassHi)
fill(dossier,162,29,5,17,brassHi); fill(dossier,156,35,17,5,brassHi)

-- Three stage pips retain their familiar position but now read as brass studs.
for i=0,2 do
  local x=164+i*28
  disc(dossier,x,120,7,brassDark)
  disc(dossier,x,120,4,(i==0) and brassHi or Color{r=81,g=49,b=26,a=255})
end
dossier:saveAs(presentationRoot.."/stage_entry_opponent_intro_frame_360x152_0_1_2_4.png")

-- Combined handoff preview using the actual Stage 1 cutout and fixed screen
-- placement: frame top-left (300,184), portrait (320,206), seal (448,84).
local stageBoardSpr=assert(app.open(boardPath),"cannot open "..boardPath)
local stagePreview=stageBoardSpr.cels[1].image:clone(); stageBoardSpr:close()
stagePreview:drawImage(seal,Point(448,84))
stagePreview:drawImage(dossier,Point(300,184))
local opponentSpr=assert(app.open(opponentPath),"cannot open "..opponentPath)
local opponent=opponentSpr.cels[1].image:clone(); opponentSpr:close()
stagePreview:drawImage(opponent,Point(320,206))
stagePreview:saveAs(previewRoot.."/bell_stage_entry_ui_preview_960x540_0_1_2_4.png")

local stageSource=Sprite(960,540,ColorMode.RGB)
stageSource.layers[1].name="bell_stage_entry_ui_preview"
stageSource.cels[1].image:drawImage(stagePreview,Point(0,0))
stageSource:saveAs(previewRoot.."/bell_stage_entry_ui_preview_960x540_0_1_2_4.aseprite")
stageSource:close()
