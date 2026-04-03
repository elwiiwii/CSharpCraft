pico-8 cartridge // http://www.pico-8.com
version 43
__lua__
--p.craft
--by nusan

lb4 = false
lb5 = false
block5 = false

time = 0

function item(n,s,p,bc)
	return {name=n,spr=s,pal=p,becraft=bc}
end

function inst(it)
	return {type=it}
end

function instc(it,c,l)
	return {type=it,count=c,list=l}
end

function setpower(v,i)
	i.power=v
	return i
end

function entity(it,xx,yy,vxx,vyy)
	return {type=it,x=xx,y=yy,vx=vxx,vy=vyy}
end

function rentity(it,xx,yy)
	return entity(it,xx,yy,rnd(3)-1.5,rnd(3)-1.5)
end

enstep_wait=0
enstep_walk=1
enstep_chase=2
enstep_patrol=3

function settext(t,c,time,e)
	e.text=t
	e.timer=time
	e.c=c
	return e
end

function bigspr(spr,ent)
	ent.bigspr=spr
	ent.drop=true
	return ent
end

function recipe(m,require)
	return {type=m.type,power=m.power,count=m.count,req=require,list=m.list}
end

function cancraft(req)
	local can=true
	for i=1,#req.req do
		if howmany(invent,req.req[i])<req.req[i].count then
			can=false
			break
		end
	end
	return can
end

function craft(req)
	for i=1,#req.req do
		reminlist(invent,req.req[i])
	end
	additeminlist(invent,setpower(req.power,instc(req.type,req.count,req.list)),0)
end

pwrnames = {"wood","stone","iron","gold","gem"}
pwrpal = {{2,2,4,4,},{5,2,4,13},{13,5,13,6},{9,2,9,10},{13,2,14,12}}

function setpal(l)
	for i=1,#l do
		pal(i,l[i])
	end
end

haxe = item("haxe",98)
sword = item("sword",99)
scythe = item("scythe",100)
shovel = item("shovel",101)
pick = item("pick",102)

pstone={0,1,5,13}
piron={1,5,13,6}
pgold={1,9,10,7}

wood = item("wood",103)
sand = item("sand",114,{15})
seed = item("seed",115)
wheat = item("wheat",118,{4,9,10,9})
apple = item("apple",116)
apple.givelife = 20
glass = item("glass",117)
stone = item("stone",118,pstone)
iron = item("iron",118,piron)
gold = item("gold",118,pgold)
gem = item("gem",118,{1,2,14,12})

fabric = item("fabric",69)
sail = item("sail",70)
glue = item("glue",85,{1,13,12,7})
boat = item("boat",86)
ichor = item("ichor",114,{11})
potion = item("potion",85,{1,2,8,14})
potion.givelife = 100

ironbar = item("iron bar",119,piron)
goldbar = item("gold bar",119,pgold)
bread = item("bread",119,{1,4,15,7})
bread.givelife = 40

workbench = bigspr(104,item("workbench",89,{1,4,9},true))
stonebench = bigspr(104,item("stonebench",89,{1,6,13},true))
furnace = bigspr(106,item("furnace",90,nil,true))
anvil = bigspr(108,item("anvil",91,nil,true))
factory = bigspr(71,item("factory",74,nil,true))
chem = bigspr(78,item("chem lab",76,nil,true))
chest = bigspr(110,item("chest",92))

inventary = item("inventory",89)
pickuptool = item("pickup tool",73)

etext = item("text",103)
player = 1
zombi = 2

grwater = {id=0,gr=0}
grsand = {id=1,gr=1}
grgrass = {id=2,gr=2}
grrock = {id=3,gr=3,mat=stone,tile=grsand,life=15}
grtree = {id=4,gr=2,mat=wood,tile=grgrass,life=8,istree=true,pal={1,5,3,11}}
grfarm = {id=5,gr=1}
grwheat = {id=6,gr=1}
grplant = {id=7,gr=2}
griron = {id=8,gr=1,mat=iron,tile=grsand,life=45,istree=true,pal={1,1,13,6}}
grgold = {id=9,gr=1,mat=gold,tile=grsand,life=80,istree=true,pal={1,2,9,10}}
grgem = {id=10,gr=1,mat=gem,tile=grsand,life=160,istree=true,pal={1,2,14,12}}
grhole = {id=11,gr=1}

lastground=grsand

grounds = {grwater,grsand,grgrass,grrock,grtree,grfarm,grwheat,grplant,griron,grgold,grgem,grhole}

function cmenu(t,l,s,te1,te2)
	return {list=l,type=t,sel=1,off=0,spr=s,text=te1,text2=te2}
end

mainmenu=cmenu(inventary,nil,128,"by nusan","2016")
intromenu=cmenu(inventary,nil,136,"a storm leaved you","on a deserted island")
deathmenu=cmenu(inventary,nil,128,"you died","alone ...")
winmenu=cmenu(inventary,nil,136,"you successfully escaped","from the island")

function howmany(list,it)
	local count=0
	for i=1,#list do
		if list[i].type==it.type then
			if not it.power or it.power==list[i].power then
				if list[i].count then
					count+=list[i].count
				else
					count+=1
				end
			end
		end
	end
	return count
end

function isinlist(list,it)
	for i=1,#list do
		if list[i].type==it.type then
			if not it.power or it.power==list[i].power then
				return list[i]
			end
		end
	end
	return nil
end

function reminlist(list,elem)
	local it=isinlist(list,elem)
	if not it then
		return
	end
	if it.count then
		it.count-=elem.count
		if it.count<=0 then
			del(list,it)
		end
	else
		del(list,it)
	end
end

function additeminlist(list,it,p)
	local it2=isinlist(list,it)
	if not it2 or not it2.count then
		addplace(list,it,p)
	else
		it2.count+=it.count
	end
end

function addplace(l,e,p)
	if p<#l and p>0 then
		for i=#l,p,-1 do
			l[i+1]=l[i]
		end
		l[p]=e
	else
		add(l,e)
	end	
end

function isin(e,size)
	return (e.x>clx-size and e.x<clx+size and e.y>cly-size and e.y<cly+size)
end

function lerp(a,b,alpha)
	return a*(1.0-alpha)+b*alpha
end

function getinvlen(x,y)
	return 1/getlen(x,y)
end

function getlen(x,y)
	return sqrt(x*x+y*y+0.001)
end

function getrot(dx,dy)
	return dy >= 0 and (dx+3) * 0.25 or (1 - dx) * 0.25
end

function normgetrot(dx,dy)
	local l = 1/sqrt(dx*dx+dy*dy+0.001)
	return getrot(dx*l,dy*l)
end

function fillene(l)
	l.ene={entity(player,0,0,0,0)}
	enemies=l.ene
	for i=0,levelsx-1 do
		for j=0,levelsy-1 do
			local c = getdirectgr(i,j)
			local r = rnd(100)
			local ex = i*16 + 8
			local ey = j*16 + 8
			local dist = max(abs(ex-plx),abs(ey-ply))
			if r<3 and c!=grwater and c!=grrock and not c.istree and dist>50 then
				local newe = entity(zombi,ex,ey,0,0)
				newe.life=10
				newe.prot=0
				newe.lrot=0
				newe.panim=0
				newe.banim=0
				newe.dtim = 0
				newe.step = 0
				newe.ox = 0
				newe.oy = 0
				add(l.ene, newe)
			end
		end
	end
end

function createlevel(xx,yy,sizex,sizey,isunderground)
	local l = {x=xx,y=yy,sx=sizex,sy=sizey,isunder=isunderground,ent={},ene={},dat={}}
	setlevel(l)
	levelunder = isunderground
	createmap()
	fillene(l)
	l.stx=(holex-levelx)*16+8
	l.sty=(holey-levely)*16+8
	return l
end

function setlevel(l)
	currentlevel=l
	levelx = l.x
	levely = l.y
	levelsx = l.sx
	levelsy = l.sy
	levelunder = l.isunder
	entities=l.ent
	enemies=l.ene
	data=l.dat
	plx=l.stx
	ply=l.sty
end

function resetlevel()

	reload()
	memcpy(0x1000,0x2000,0x1000)

	prot = 0
	lrot = 0

	panim = 0

	pstam = 100
	lstam = pstam
	plife = 100
	llife = plife

	banim = 0

	coffx = 0
	coffy = 0

	time = 0

	tooglemenu=0
	invent = {}
	curitem = nil
	switchlevel = false
	canswitchlevel = false
	menuinvent=cmenu(inventary,invent)

	for i=0,15 do
		rndwat[i] = {}
		for j=0,15 do
			rndwat[i][j] = rnd(100)
		end
	end
	
	cave = createlevel(64,0,32,32,true)
	island = createlevel(0,0,64,64,false)

	local tmpworkbench = entity(workbench,plx,ply,0,0)
	tmpworkbench.hascol=true
	tmpworkbench.list = workbenchrecipe
	
	add(invent,tmpworkbench)
	add(invent,inst(pickuptool))

	-- cheat, to remove

	--local tmpchest = entity(chest,plx+16,ply,0,0)
	--tmpchest.hascol=true
	--tmpchest.list = {}
	--local itl = {haxe,pick,sword,shovel,scythe}
	--for i=1,#itl do
	--	for j=1,5 do
	--		add(tmpchest.list, setpower(j, inst(itl[i])))
	--	end
	--end
	--add(entities,tmpchest)

	--add(invent,instc(wood,540))
	--add(invent,instc(stone,300))
	--add(invent,instc(iron,300))
	--add(invent,instc(wheat,300))
	--add(invent,instc(gold,300))


end

function _init()

	music(4,10000)

	furnacerecipe={}
	workbenchrecipe={}
	stonebenchrecipe={}
	anvilrecipe={}
	factoryrecipe={}
	chemrecipe={}

	add(factoryrecipe,recipe(instc(sail,1),{instc(fabric,3),instc(glue,1)}))
	add(factoryrecipe,recipe(inst(boat),{instc(wood,30),instc(ironbar,8),instc(glue,5),instc(sail,4)}))
	
	add(chemrecipe,recipe(instc(glue,1),{instc(glass,1),instc(ichor,3)}))
	add(chemrecipe,recipe(instc(potion,1),{instc(glass,1),instc(ichor,1)}))

	add(furnacerecipe,recipe(instc(ironbar,1),{instc(iron,3)}))
	add(furnacerecipe,recipe(instc(goldbar,1),{instc(gold,3)}))
	add(furnacerecipe,recipe(instc(glass,1),{instc(sand,3)}))
	add(furnacerecipe,recipe(instc(bread,1),{instc(wheat,5)}))

	local tooltypes = {haxe,pick,sword,shovel,scythe}
	local quant = {5,5,7,7,7}
	local pows = {1,2,3,4,5}
	local materials = {wood,stone,ironbar,goldbar,gem}
	local mult = {1,1,1,1,3}
	local crafter = {workbenchrecipe,stonebenchrecipe,anvilrecipe,anvilrecipe,anvilrecipe}
	for j=1,#pows do
		for i=1,#tooltypes do
			add(crafter[j],recipe(setpower(pows[j],inst(tooltypes[i])),{instc(materials[j],quant[i]*mult[j])}))
		end
	end

	add(workbenchrecipe,recipe(instc(workbench,nil,workbenchrecipe),{instc(wood,15)}))
	add(workbenchrecipe,recipe(instc(stonebench,nil,stonebenchrecipe),{instc(stone,15)}))
	add(workbenchrecipe,recipe(instc(factory,nil,factoryrecipe),{instc(wood,15),instc(stone,15)}))
	add(workbenchrecipe,recipe(instc(chem,nil,chemrecipe),{instc(wood,10),instc(glass,3),instc(gem,10)}))
	add(workbenchrecipe,recipe(inst(chest),{instc(wood,15),instc(stone,10)}))

	add(stonebenchrecipe,recipe(instc(anvil,nil,anvilrecipe),{instc(iron,25),instc(wood,10),instc(stone,25)}))
	add(stonebenchrecipe,recipe(instc(furnace,nil,furnacerecipe),{instc(wood,10),instc(stone,15)}))

	--srand(1245)

	curmenu = mainmenu

end

function getmcoord(x,y)
	return flr(x/16),flr(y/16)
end

function isfree(x,y)
	local gr = getgr(x,y)
	return not (gr.istree or gr==grrock)
end

function isfreeenem(x,y)
	local gr = getgr(x,y)
	return not (gr.istree or gr==grrock or gr==grwater)
end

function iscool(x,y)
	return not isfree(x,y)
end

function getgr(x,y)
	local i,j = getmcoord(x,y)
	return getdirectgr(i,j)
end

function getdirectgr(i,j)
	if(i<0 or j<0 or i>=levelsx or j>=levelsy) return grounds[1]
	return grounds[mget(i+levelx,j)+1]
end

function setgr(x,y,v)
	local i,j = getmcoord(x,y)
	if(i<0 or j<0 or i>=levelsx or j>=levelsy) return
	mset(i+levelx,j,v.id)
end

function dirgetdata(i,j,default)
	local g = i+j*levelsx
	if data[g]==nil then
		data[g] = default
	end
	return data[g]
end

function dirsetdata(i,j,v)
	data[i+j*levelsx] = v
end

function getdata(x,y,default)
	local i,j = getmcoord(x,y)
	if i<0 or j<0 or i>levelsx-1 or j>levelsy-1 then
		return default
	end
	return dirgetdata(i,j,default)
end

function setdata(x,y,v)
	local i,j = getmcoord(x,y)
	if i<0 or j<0 or i>levelsx-1 or j>levelsy-1 then
		return
	end
	dirsetdata(i,j,v)
end

function cleardata(x,y)
	local i,j = getmcoord(x,y)
	if i<0 or j<0 or i>levelsx-1 or j>levelsy-1 then
		return
	end
	data[i+j*levelsx] = nil
end

function loop(sel,l)
	local lp = #l
	return (((sel-1)%lp)+lp)%lp+1
end

function entcolfree(x,y,e)
	return max(abs(e.x-x),abs(e.y-y))>8
end

function reflectcol(x,y,dx,dy,checkfun,dp,e)

	local newx = x + dx
	local newy = y + dy

	local ccur = checkfun(x,y,e)
	local ctotal = checkfun(newx,newy,e)
	local chor = checkfun(newx,y,e)
	local cver = checkfun(x,newy,e)

	if ccur then
		if chor or cver then
			if not ctotal then
				if chor then
					dy = -dy*dp
				else
					dx = -dx*dp
				end
			end
		else
			dx=-dx*dp
			dy=-dy*dp
		end
	end

	return dx,dy
end

function additem(mat,count,hitx,hity)
	for i=1,count do
		local gi = rentity(mat,flr(hitx/16)*16 + rnd(14)+1,flr(hity/16)*16 + rnd(14)+1)
		gi.giveitem = mat
		gi.hascol = true
		gi.timer = 110+rnd(20)
		add(entities,gi)
	end
end

function upground()

	local ci = flr((clx-64)/16)
	local cj = flr((cly-64)/16)
	for i=ci,ci+8 do
		for j=cj,cj+8 do
			local gr = getdirectgr(i,j)
			if gr==grfarm then
				local d = dirgetdata(i,j,0)
				if time>d then
					mset(i+levelx,j,grsand.id)
				end
			end
		end
	end
end

function uprot(grot,rot)
	if abs(rot-grot) > 0.5 then 
		if rot>grot then
			grot += 1
		else
			grot -= 1
		end
	end
	return (lerp(rot, grot, 0.4)%1+1)%1
end

function _update()

	if curmenu then
		if curmenu.spr then
			if btnp(4) and not lb4 then
				if(curmenu==mainmenu) then
					curmenu=intromenu
				else
					resetlevel()
					curmenu=nil
					music(1)
				end
			end
			lb4 = btn(4)
			return
		else
			local intmenu = curmenu
			local othmenu = menuinvent
			if curmenu.type==chest then
				if(btnp(0)) then tooglemenu-=1 sfx(18,3) end
				if(btnp(1)) then tooglemenu+=1 sfx(18,3) end
				tooglemenu=(tooglemenu%2+2)%2
				if tooglemenu==1 then
					intmenu = menuinvent
					othmenu = curmenu
				end
			end

			if #intmenu.list>0 then
				if(btnp(2)) then intmenu.sel-=1 sfx(18,3) end
				if(btnp(3)) then intmenu.sel+=1 sfx(18,3) end
				
				intmenu.sel = loop(intmenu.sel,intmenu.list)

				if btnp(5) and not lb5 then
					if curmenu.type==chest then
						sfx(16,3)
						local el = intmenu.list[intmenu.sel]
						del(intmenu.list,el)
						additeminlist(othmenu.list,el,othmenu.sel)
						if(#intmenu.list>0 and intmenu.sel>#intmenu.list) intmenu.sel-=1
						if intmenu==menuinvent and curitem==el then
							curitem=nil
						end
					elseif curmenu.type.becraft then
						if curmenu.sel>0 and curmenu.sel<=#curmenu.list then
							local rec = curmenu.list[curmenu.sel]
							if cancraft(rec) then
								craft(rec)
								sfx(16,3)
							else
								sfx(17,3)
							end
						end
					else
						curitem = curmenu.list[curmenu.sel]
						del(curmenu.list,curitem)
						additeminlist(curmenu.list,curitem,1)
						curmenu.sel=1
						curmenu=nil
						block5=true
						sfx(16,3)
					end
				end
			end
		end
		if btnp(4) and not lb4 then
			curmenu=nil
			sfx(17,3)
		end
		lb4 = btn(4)
		lb5 = btn(5)
		return
	end

	if switchlevel then
		if currentlevel==cave then setlevel(island)
		else setlevel(cave) end
		plx = currentlevel.stx
		ply = currentlevel.sty
		fillene(currentlevel)
		switchlevel=false
		canswitchlevel=false
		music(currentlevel==cave and 4 or 1)
	end

	if curitem then
		if(howmany(invent,curitem)<=0) curitem=nil
	end

	upground()

	
	local playhit = getgr(plx,ply)
	if(playhit!=lastground and playhit==grwater) sfx(11,3)
	lastground = playhit
	local s = (playhit==grwater or pstam<=0) and 1 or 2
	if playhit==grhole then
		switchlevel = switchlevel or canswitchlevel
	else
		canswitchlevel = true
	end

	local dx = 0
	local dy = 0

	if(btn(0)) dx -= 1
	if(btn(1)) dx += 1
	if(btn(2)) dy -= 1
	if(btn(3)) dy += 1

	local dl = getinvlen(dx,dy)

	dx *= dl
	dy *= dl

	if abs(dx)>0 or abs(dy)>0 then
		lrot = getrot(dx,dy)
		panim += 1/33
	else
		panim = 0
	end

	dx *= s
	dy *= s

	dx,dy = reflectcol(plx,ply,dx,dy,isfree,0)

	local canact = true

	local fin=#entities
	for i=fin,1,-1 do
		local e = entities[i]
		if e.hascol then
			e.vx,e.vy = reflectcol(e.x,e.y,e.vx,e.vy,isfree,0.9)
		end
		e.x += e.vx
		e.y += e.vy
		e.vx *= 0.95
		e.vy *= 0.95

		if e.timer and e.timer<1 then
			del(entities,e)
		else
			if(e.timer) e.timer-=1

			local dist = max(abs(e.x-plx),abs(e.y-ply))
			if e.giveitem then
				if dist<5 then
					if not e.timer or e.timer<115 then
						local newit = instc(e.giveitem,1)
						additeminlist(invent,newit,-1)
						del(entities,e)
						add(entities,settext(howmany(invent,newit),11,20,entity(etext,e.x,e.y-5,0,-1)))
						sfx(18,3)
					end
				end
			else
				if e.hascol then
					dx,dy = reflectcol(plx,ply,dx,dy,entcolfree,0,e)
				end
				if dist<12 and btn(5) and not block5 and not lb5 then
					if curitem and curitem.type==pickuptool then
						if e.type==chest or e.type.becraft then
							additeminlist(invent,e,0)
							curitem=e
							del(entities,e)
						end
						canact = false
					else
						if e.type==chest or e.type.becraft then
							tooglemenu=0
							curmenu = cmenu(e.type,e.list)
							sfx(13,3)
						end
						canact = false
					end
				end
			end
		end
	end

	nearenemies={}

	local ebx = cos(prot)
	local eby = sin(prot)

	for i=1,#enemies do
		local e = enemies[i]
		if isin(e,100) then
			if e.type == player then
				e.x=plx
				e.y=ply
			else
				local distp = getlen(e.x-plx,e.y-ply)
				local mspeed = 0.8

				local disten = getlen(e.x-plx - ebx*8,e.y-ply - eby*8)
				if disten<10 then
					add(nearenemies,e)
				end
				if distp<8 then
					e.ox += max(-0.4,min(0.4,e.x-plx))
					e.oy += max(-0.4,min(0.4,e.y-ply))
				end

				if e.dtim<=0 then
					if e.step==enstep_wait or e.step==enstep_patrol then
						e.step=enstep_walk
						e.dx = rnd(2)-1
						e.dy = rnd(2)-1				
						e.dtim = 30+rnd(60)
					elseif e.step==enstep_walk then
						e.step=enstep_wait
						e.dx=0
						e.dy=0
						e.dtim = 30+rnd(60)
					else -- chase
						e.dtim = 10+rnd(60)
					end
				else
					if e.step==enstep_chase then
						if distp>10 then
							e.dx += plx-e.x
							e.dy += ply-e.y
							e.banim = 0
						else
							e.dx = 0
							e.dy = 0
							e.banim -= 1
							e.banim = e.banim%8
							local pow = 10
							if e.banim==4 then
								plife -= pow
								add(entities,settext(pow,8,20,entity(etext,plx,ply-10,0,-1)))
								sfx(14+rnd(2),3)
							end
							plife = max(0,plife)
						end
						mspeed = 1.4
						if distp>70 then
							e.step=enstep_patrol
							e.dtim = 30+rnd(60)
						end
					else
						if distp<40 then
							e.step=enstep_chase
							e.dtim = 10+rnd(60)
						end
					end
					e.dtim -= 1
				end

				local dl = mspeed*getinvlen(e.dx,e.dy)
				e.dx *= dl
				e.dy *= dl

				local fx = e.dx+e.ox
				local fy = e.dy+e.oy
				fx,fy = reflectcol(e.x,e.y,fx,fy,isfreeenem,0)

				if abs(e.dx)>0 or abs(e.dy)>0 then
					e.lrot = getrot(e.dx,e.dy)
					e.panim += 1/33
				else
					e.panim = 0
				end

				e.x += fx
				e.y += fy

				e.ox *= 0.9
				e.oy *= 0.9

				e.prot = uprot(e.lrot,e.prot)
			end
		end
	end

	dx,dy = reflectcol(plx,ply,dx,dy,isfree,0)

	plx += dx
	ply += dy

	prot = uprot(lrot,prot)

	llife += max(-1,min(1,(plife-llife)))
	lstam += max(-1,min(1,(pstam-lstam)))
		
	if btn(5) and not block5 and canact then
		local bx = cos(prot)
		local by = sin(prot)
		local hitx = plx + bx * 8
		local hity = ply + by * 8
		local hit = getgr(hitx,hity)

		if not lb5 and curitem and curitem.type.drop then
			if hit == grsand or hit == grgrass then
				
				if(not curitem.list) curitem.list={}
				curitem.hascol=true

				curitem.x = flr(hitx/16)*16+8
				curitem.y = flr(hity/16)*16+8
				curitem.vx = 0
				curitem.vy = 0
				add(entities,curitem)
				reminlist(invent,curitem)
				canact = false
			end
		end

		if banim==0 and pstam>0 and canact then
			banim = 8
			stamcost = 20
			if #nearenemies>0 then
				sfx(19,3)
				local pow = 1
				if curitem and curitem.type==sword then
					pow = 1+curitem.power+rnd(curitem.power*curitem.power)
					stamcost = max(0,20-curitem.power*2)
					pow=flr(pow)
					sfx(14+rnd(2),3)
				end
				for i=1,#nearenemies do
					local e = nearenemies[i]
					e.life -= pow/#nearenemies
					local push = (pow-1)*0.5
					e.ox += max(-push,min(push,e.x-plx))
					e.oy += max(-push,min(push,e.y-ply))
					if e.life<=0 then
						del(enemies,e)
						additem(ichor,rnd(3),e.x,e.y)
						additem(fabric,rnd(3),e.x,e.y)
					end
					add(entities,settext(pow,9,20,entity(etext,e.x,e.y-10,0,-1)))
				end
			elseif hit.mat then
				sfx(15,3)
				local pow = 1
				if curitem then
					if hit==grtree then
					 	if curitem.type==haxe then
							pow = 1+curitem.power+rnd(curitem.power*curitem.power)
							stamcost = max(0,20-curitem.power*2)
							sfx(12,3)
						end						
					elseif (hit==grrock or hit.istree) and curitem.type==pick then
						pow = 1+curitem.power*2+rnd(curitem.power*curitem.power)
						stamcost = max(0,20-curitem.power*2)
						sfx(12,3)
					end
				end
				pow=flr(pow)

				local d = getdata(hitx,hity,hit.life)
				if d-pow<=0 then
					setgr(hitx,hity,hit.tile)
					cleardata(hitx,hity)
					additem(hit.mat,rnd(3)+2,hitx,hity)
					if hit==grtree and rnd()>0.7 then
						additem(apple,1,hitx,hity)
					end
				else
					setdata(hitx,hity,d-pow)
				end
				add(entities,settext(pow,10,20,entity(etext,hitx,hity,0,-1)))
			else
				sfx(19,3)
				if curitem then
					if curitem.power then
						stamcost = max(0,20-curitem.power*2)
					end
					if curitem.type.givelife then
						plife = min(100,plife+curitem.type.givelife)
						reminlist(invent,instc(curitem.type,1))
						sfx(21,3)
					end
					if hit==grgrass and curitem.type==scythe then
						setgr(hitx,hity,grsand)
						if(rnd()>0.4) additem(seed,1,hitx,hity)
					end
					if hit==grsand and curitem.type==shovel then
						if curitem.power>3 then
							setgr(hitx,hity,grwater)
							additem(sand,2,hitx,hity)
						else
							setgr(hitx,hity,grfarm)
							setdata(hitx,hity,time+15+rnd(5))
							additem(sand,rnd(2),hitx,hity)
						end
					end
					if hit==grwater and curitem.type==sand then
						setgr(hitx,hity,grsand)
						reminlist(invent,instc(sand,1))
					end
					if hit==grwater and curitem.type==boat then
						reload()
						memcpy(0x1000,0x2000,0x1000)
						curmenu=winmenu
						music(4)
					end
					if hit==grfarm and curitem.type==seed then
						setgr(hitx,hity,grwheat)
						setdata(hitx,hity,time+15+rnd(5))
						reminlist(invent,instc(seed,1))
					end
					if hit==grwheat and curitem.type==scythe then
						setgr(hitx,hity,grsand)
						local d = max(0,min(4,4-(getdata(hitx,hity,0)-time)))
						additem(wheat,d/2+rnd(d/2),hitx,hity)
						additem(seed,1,hitx,hity)
					end
				end
			end
			pstam -= stamcost
		end
	end

	if banim>0 then
		banim -= 1
	end

	if pstam<100 then
		pstam = min(100,pstam+1)
	end

	local m = 16
	local msp = 4

	if abs(cmx-plx)>m then
		coffx += dx*0.4
	end
	if abs(cmy-ply)>m then
		coffy += dy*0.4
	end

	cmx = max(plx-m,cmx)
	cmx = min(plx+m,cmx)
	cmy = max(ply-m,cmy)
	cmy = min(ply+m,cmy)

	coffx *= 0.9
	coffy *= 0.9
	coffx = min(msp,max(-msp,coffx))
	coffy = min(msp,max(-msp,coffy))

	clx += coffx
	cly += coffy

	clx = max(cmx-m,clx)
	clx = min(cmx+m,clx)
	cly = max(cmy-m,cly)
	cly = min(cmy+m,cly)

	if btnp(4) and not lb4 then
		curmenu=menuinvent
		sfx(13,3)
	end

	lb4 = btn(4)
	lb5 = btn(5)
	if not btn(5) then
		block5=false
	end

	time += 1/30

	if(plife<=0) then
		reload()
		memcpy(0x1000,0x2000,0x1000)
		curmenu=deathmenu
		music(4)
	end

end

function mirror(rot)
	if rot<0.125 then
		return 0,1
	elseif rot<0.325 then
	elseif rot<0.625 then
		return 1,0
	elseif rot<0.825 then
		return 1,1
	else
		return 0,1
	end
	return 0,0
end

function dplayer(x,y,rot,anim,subanim,isplayer)

	local cr = cos(rot)
	local sr = sin(rot)
	local cv = -sr
	local sv = cr

	x = flr(x)
	y = flr(y-4)

	local lan = sin(anim*2)*1.5	
	local bel = getgr(x,y)
	if bel==grwater then
		y += 4
		circ(x + cv*3 + cr * lan,y + sv*3 + sr * lan,3,6)
		circ(x - cv*3 - cr * lan,y - sv*3 - sr * lan,3,6)
	
		local anc = 3 + ((time*3)%1)*3
		circ(x + cv*3 + cr * lan,y + sv*3 + sr * lan,anc,6)
		circ(x - cv*3 - cr * lan,y - sv*3 - sr * lan,anc,6)
	else
			
		circfill(x + cv*2 - cr * lan,y + 3 + sv*2 - sr * lan,3,1)
		circfill(x - cv*2 + cr * lan,y + 3 - sv*2 + sr * lan,3,1)
	end
		local blade = (rot+0.25)%1
		if subanim>0 then
			blade = blade - 0.3 + subanim*0.04
		end
		local bcr = cos(blade)
		local bsr = sin(blade)

		local mx,my = mirror(blade)

		local weap = 75

		if isplayer and curitem then
			pal()
			weap=curitem.type.spr
			if curitem.power then
				setpal(pwrpal[curitem.power])
			end
			if curitem.type and curitem.type.pal then
				setpal(curitem.type.pal)
			end
		end

		spr(weap,x + bcr*4 - cr * lan - mx*8 + 1, y + bsr*4 - sr * lan + my*8 - 7,1,1,mx==1,my==1)

		if(isplayer) pal()
	
	if bel!=grwater then
		circfill(x + cv*3 + cr * lan,y + sv*3 + sr * lan,3,2)
		circfill(x - cv*3 - cr * lan,y - sv*3 - sr * lan,3,2)
		
		local my2,mx2 = mirror((rot+0.75)%1)
		spr(75,x + cv*4 + cr * lan -8+mx2*8 + 1, y + sv*4 + sr * lan + my2*8 - 7,1,1,mx2==0,my2==1)

	end
	
	circfill(x+cr,y+sr-2,4,2)
	circfill(x+cr,y+sr,4,2)
	circfill(x+cr*1.5,y+sr*1.5-2,2.5,15)
	circfill(x-cr,y-sr-3,3,4)

end

function noise(sx,sy,startscale,scalemod,featstep)

	local n = {}
	for i=0,sx do
		n[i] = {}
		for j=0,sy do
			n[i][j] = 0.5
		end
	end

	local step = sx
	local scale = startscale
	while step>1 do
		local cscal = scale
		if(step == featstep) cscal = 1
		for i=0,sx-1,step do
			for j=0,sy-1,step do
				local c1 = n[i][j]
				local c2 = n[i+step][j]
				local c3 = n[i][j+step]
				n[i+step/2][j] = (c1+c2)*0.5 + (rnd()-0.5)*cscal
				n[i][j+step/2] = (c1+c3)*0.5 + (rnd()-0.5)*cscal
			end
		end
		for i=0,sx-1,step do
			for j=0,sy-1,step do
				local c1 = n[i][j]
				local c2 = n[i+step][j]
				local c3 = n[i][j+step]
				local c4 = n[i+step][j+step]
				n[i+step/2][j+step/2] = (c1+c2+c3+c4)*0.25 + (rnd()-0.5)*cscal
			end
		end
		step /= 2
		scale *= scalemod

	end

	return n
end

level = {}
typecount = {}

function createmapstep(sx,sy,a,b,c,d,e)

	local cur = noise(sx,sy,0.9,0.2,sx)
	local cur2 = noise(sx,sy,0.9,0.4,8)
	local cur3 = noise(sx,sy,0.9,0.3,8)
	local cur4 = noise(sx,sy,0.8,1.1,4)

	for i=0,11 do
		typecount[i] = 0
	end

	for i=0,sx do
		for j=0,sy do
			local v = abs(cur[i][j] - cur2[i][j])
			local v2 = abs(cur[i][j] - cur3[i][j])
			local v3 = abs(cur[i][j] - cur4[i][j])
			local dist = max((abs(i/sx - 0.5) * 2), (abs(j/sy - 0.5) * 2))
			dist = dist*dist*dist*dist
			local coast = v*4 - dist*4

			local id = a
			if(coast>0.3) id = b -- sand
			if(coast>0.6) id = c -- grass
			if(coast>0.3 and v2>0.5) id = d -- stone
			if(id == c and v3>0.5) id = e -- tree

			typecount[id] += 1

			cur[i][j] = id
		end
	end

	return cur
end

function createmap()

	local needmap = true

	while needmap do

		needmap = false

		if levelunder then
			level = createmapstep(levelsx,levelsy,3,8,1,9,10)

			if(typecount[8]<30) needmap = true
			if(typecount[9]<20) needmap = true
			if(typecount[10]<15) needmap = true
		else
			level = createmapstep(levelsx,levelsy,0,1,2,3,4)

			if(typecount[3]<30) needmap = true
			if(typecount[4]<30) needmap = true
		end

		if not needmap then
			plx = -1
			ply = -1
			for i=0,500 do
				local depx = flr(levelsx/8+rnd(levelsx*6/8))
				local depy = flr(levelsy/8+rnd(levelsy*6/8))
				local c = level[depx][depy]
				if c == 1 or c == 2 then
					plx = depx*16 + 8
					ply = depy*16 + 8
					break
				end
			end
			if plx < 0 then
				needmap = true
			end
		end
	end

	for i=0,levelsx-1 do
		for j=0,levelsy-1 do
			mset(i+levelx,j+levely,level[i][j])
		end
	end

	holex = levelsx/2+levelx
	holey = levelsy/2+levely
	for i=-1,1 do
		for j=-1,1 do
			mset(holex+i,holey+j,((levelunder) and 1 or 3))
		end
	end
	mset(holex,holey,11)
	
	clx = plx
	cly = ply

	cmx = plx
	cmy = ply
	
end

function comp(i,j,gr)
	local gr2 = getdirectgr(i,j)
	return (gr and gr2 and gr.gr == gr2.gr)
end

rndwat = {}

function watval(i,j)
	return rndwat[flr((i*2)%16)][flr((j*2)%16)]
end

function watanim(i,j)
	local a = ((time*0.6 + watval(i,j)/100)%1) * 19
	if(a>16) spr(13+a-16,i*16,j*16)
end

function rndcenter(i,j)
	return (flr(watval(i,j)/34)+18)%20
end

function rndsand(i,j)
	return flr(watval(i,j)/34)+1
end

function rndtree(i,j)
	return flr(watval(i,j)/51)*32
end

function spr4(i,j,gi,gj,a,b,c,d,off,f)
	spr(f(i,j+off)+a,gi,gj+2*off)
	spr(f(i+0.5,j+off)+b,gi+8,gj+2*off)
	spr(f(i,j+0.5+off)+c,gi,gj+8+2*off)
	spr(f(i+0.5,j+0.5+off)+d,gi+8,gj+8+2*off)
end

function drawback()

	local ci = flr((clx-64)/16)
	local cj = flr((cly-64)/16)
	for i=ci,ci+8 do
		for j=cj,cj+8 do
			local gr = getdirectgr(i,j)

			local gi = (i-ci)*2 + 64
			local gj = (j-cj)*2 + 32

			if gr and gr.gr == 1 then -- sand
				local sv=0
				if(gr==grfarm or gr==grwheat) sv=3
				mset(gi,gj,rndsand(i,j)+sv)
				mset(gi+1,gj,rndsand(i+0.5,j)+sv)
				mset(gi,gj+1,rndsand(i,j+0.5)+sv)
				mset(gi+1,gj+1,rndsand(i+0.5,j+0.5)+sv)
			else

				local u = comp(i,j-1, gr)
				local d = comp(i,j+1, gr)
				local l = comp(i-1,j, gr)
				local r = comp(i+1,j, gr)

				local b = gr==grrock and 21 or gr==grwater and 26 or 16
	
				mset(gi,gj,b + (l and (u and (comp(i-1,j-1, gr) and 17+rndcenter(i,j) or 20) or 1) or (u and 16 or 0)) )
				mset(gi+1,gj,b + (r and (u and (comp(i+1,j-1, gr) and 17+rndcenter(i+0.5,j) or 19) or 1) or (u and 18 or 2)) )				
				mset(gi,gj+1,b + (l and (d and (comp(i-1,j+1, gr) and 17+rndcenter(i,j+0.5) or 4) or 33) or (d and 16 or 32)) )
				mset(gi+1,gj+1,b + (r and (d and (comp(i+1,j+1, gr) and 17+rndcenter(i+0.5,j+0.5) or 3) or 33) or (d and 18 or 34)) )

			end
		end
	end

	pal()
	if levelunder then
		pal(15,5)
		pal(4,1)
	end
	map(64,32,ci*16,cj*16,18,18)

	for i=ci-1,ci+8 do
		for j=cj-1,cj+8 do
			local gr = getdirectgr(i,j)
			if gr then
				local gi = i*16
				local gj = j*16

				pal()
					
				if gr==grwater then
					watanim(i,j)
					watanim(i+0.5,j)
					watanim(i,j+0.5)
					watanim(i+0.5,j+0.5)
				end

				if gr==grwheat then
					local d = dirgetdata(i,j,0)-time
					for pp=2,4 do
						pal(pp,3)
						if(d>(10-pp*2)) palt(pp,true)
					end
					if(d<0) pal(4,9)
					spr4(i,j,gi,gj,6,6,6,6,0,rndsand)
				end
				
				if gr.istree then
					setpal(gr.pal)

					spr4(i,j,gi,gj,64,65,80,81,0,rndtree)
					
					if mget(i+levelx,j+1) == 'c' then
						spr4(i,j,gi,gj,64,65,80,81,4,rndtree)
					end
				end

				if gr==grhole then
					pal()
					if not levelunder then
						palt(0,false)
						spr(31,gi,gj,1,2)
						spr(31,gi+8,gj,1,2,true)
					end
					palt()
					spr(77,gi+4,gj,1,2)
				end
			end
		end
	end
end

function panel(name,x,y,sx,sy)
	rectfill(x+8,y+8,x+sx-9,y+sy-9,1)
	spr(66,x,y)
	spr(67,x+sx-8,y)
	spr(82,x,y+sy-8)
	spr(83,x+sx-8,y+sy-8)
	sspr(24,32,4,8,x+8,y,sx-16,8)
	sspr(24,40,4,8,x+8,y+sy-8,sx-16,8)
	sspr(16,36,8,4,x,y+8,8,sy-16)
	sspr(24,36,8,4,x+sx-8,y+8,8,sy-16)

	local hx = x+(sx-#name*4)/2
	rectfill(hx,y+1,hx+#name*4,y+7,13)
	print(name,hx+1,y+2,7)
end

function itemname(x,y,it,col)
	local ty = it.type
	pal()
	local px = x
	if it.power then
		local pwn = pwrnames[it.power]
		print(pwn,x+10,y,col)
		px += #pwn*4 + 4
		setpal(pwrpal[it.power])
	end
	if(ty.pal) setpal(ty.pal)
	spr(ty.spr,x,y-2)
	pal()
	print(ty.name,px+10,y,col)
end

function list(menu,x,y,sx,sy,my)
	panel(menu.type.name,x,y,sx,sy)

	local tlist = #menu.list
	if tlist<1 then
		return
	end

	local sel = menu.sel
	if(menu.off>max(0,sel-4)) menu.off=max(0,sel-4)
	if(menu.off<min(tlist,sel+3)-my) menu.off=min(tlist,sel+3)-my

	sel -= menu.off

	local debut = menu.off+1
	local fin = min(menu.off+my,tlist)

	local sely = y+3+sel*8
	rectfill(x+1,sely,x+sx-3,sely+6,13)

	x+=5
	y+=12

	for i=debut,fin do
		local it = menu.list[i]
		local py = y+(i-1-menu.off)*8
		local col = 7
		if it.req and not cancraft(it) then
			col = 0
		end

		itemname(x,py,it,col)

		if it.count then
			local c = ""..it.count
			print(c,x+sx-#c*4-10,py,col)
		end
	end

	spr(68,x-8,sely)
	spr(68,x+sx-10,sely,1,1,true)
end

function requirelist(recip,x,y,sx,sy)
	panel("require",x,y,sx,sy)
	local tlist = #recip.req
	if tlist<1 then
		return
	end

	x+=5
	y+=12

	for i=1,tlist do
		local it = recip.req[i]
		local py = y+(i-1)*8
		itemname(x,py,it,7)

		if it.count then
			local h = howmany(invent,it)
			local c = h.."/"..it.count
			print(c,x+sx-#c*4-10,py,h<it.count and 8 or 7)
		end
	end
	
end

function printb(t,x,y,c)
	print(t,x+1,y,1)
	print(t,x-1,y,1)
	print(t,x,y+1,1)
	print(t,x,y-1,1)
	print(t,x,y,c)
end

function printc(t,x,y,c)
	print(t,x-#t*2,y,c)
end

function dent()
	for i=1,#entities do
		local e = entities[i]
		pal()
		if(e.type.pal) setpal(e.type.pal)
		if e.type.bigspr then
			spr(e.type.bigspr,e.x-8,e.y-8,2,2)
		else
			if e.type == etext then
				printb(e.text,e.x-2,e.y-4,e.c)
			else
				if e.timer and e.timer<45 and e.timer%4>2 then
					for i=0,15 do
						palt(i,true)
					end
				end
				spr(e.type.spr,e.x-4,e.y-4)
			end
		end
	end
end

function sorty(t)
	local tv = #t-1
	for i=1,tv do
		local t1 = t[i]
		local t2 = t[i+1]
		if t1.y > t2.y then
			t[i] = t2
			t[i+1] = t1
		end
	end
end

function denemies()
	sorty(enemies)

	for i=1,#enemies do
		local e = enemies[i]
		if e.type == player then	
			pal()
			dplayer(plx,ply,prot,panim,banim,true)
		else
			if isin(e,72) then
				pal()
				pal(15,3)
				pal(4,1)
				pal(2,8)
				pal(1,1)

				dplayer(e.x,e.y,e.prot,e.panim,e.banim,false)
			end
		end
	end
end

function dbar(px,py,v,m,c,c2)
	pal()
	local pe = px+v*0.3
	local pe2 = px+m*0.3
	rectfill(px-1,py-1,px+30,py+4,0)
	rectfill(px,py,pe,py+3,c2)
	rectfill(px,py,max(px,pe-1),py+2,c)
	if(m>v) rectfill(pe+1,py,pe2,py+3,10)
end

function _draw()

	if curmenu and curmenu.spr then
		camera()
		palt(0,false)
		rectfill(0,0,128,46,12)
		rectfill(0,46,128,128,1)
		spr(curmenu.spr,32,14,8,8)
		printc(curmenu.text, 64,80,6)
		printc(curmenu.text2, 64,90,6)
		printc("press button 1", 64,112,6+time%2)
		time += 0.1
		return
	end

	cls()

	camera(clx-64, cly-64)

	drawback()

	dent()

	denemies()

	camera()
	dbar(4,4,plife,llife,8,2)
	dbar(4,9,max(0,pstam),lstam,11,3)

	if curitem then
		local ix = 35
		local iy = 3
		itemname(ix+1,iy+3,curitem,7)
		if curitem.count then
			local c = ""..curitem.count
			print(c,ix+88-16,iy+3,7)
		end
	end

	if curmenu then
		camera()
		if curmenu.type==chest then
			if tooglemenu==0 then
				list(menuinvent,87,24,84,96,10)
				list(curmenu,4,24,84,96,10)
			else
				list(curmenu,-44,24,84,96,10)
				list(menuinvent,39,24,84,96,10)
			end
		elseif curmenu.type.becraft then
			if curmenu.sel>=1 and curmenu.sel<=#curmenu.list then
				local curgoal = curmenu.list[curmenu.sel]
				panel("have",71,50,52,30)
				print(howmany(invent,curgoal),91,65,7)
				requirelist(curgoal,4,79,104,50)
			end
			list(curmenu,4,16,68,64,6)
		else
			list(curmenu,4,24,84,96,10)
		end
	end

	--if(true) then
	--	print("cpu "..flr(stat(1)*100),96,0,8)
	--	print("ram "..flr(stat(0)),96,8,8)
	--end

end

__gfx__
00000000ffffffffffffffffffffffffffffffff44fff44ffff44fff020121000004200002031000fff55fffffff555ff5555fff000000000001000000101000
00000000ffffffffffffffffffffffffffff444fff4ffff4ff4fffff310310200303102041420000ff56655ffff56665f56665ff000100000011100001000100
00000000fff4fffff4ffffffff4fffff4444fffffff444ff44fff44f205200024002001030310410f566665ffff566655666665f001110000110110010000100
00000000ffffffffffffff4ffffffffffffffff4ff4fff44fff44ff415340401340100402020030256666665f551566515666665000100000011100000000000
00000000ffffffffffffffffffffff4fff44444fffffffffffffffff424243032300403410140201566666655665155115666665000000000001000001100100
00000000ffffffffff4fffffffffffffffffffffff44fff444fff44f313132021240302300034104156655515666511ff1566565000000000000000000010000
00000000ffff4ffffffffffffffffffff4444fff44ff444fff444fff002021404130201204023003f155511f56651ffff1565151000000000000000000000000
00000000fffffffffffffffffffffffffffff444ffffffffffffffff001010000020100003012040ff111fff1551ffffff151f1f000000000000000000000000
fffff11ffffffffff11fffff3353333333333333ff1111ffff1111ffff1111ff6666666666666666fffff44444ffff444444ffffddddddddddddddddffffffff
fff115511fffff1115511fff3515333333333353f155551111555511115555df6666666666666666ff44444444444444444444ffddddddddddddddddfffff111
ff15533551fff155533551ff5153333333333515155555555555555555555dd166666dddddd66666f4444444444444444444444fddddddddddddddddfff11666
f15333333511153333333b1f515333333353515315556555555665555556ddd1666dddddddddd66644441111144444411444444fddddddddddddddddff166666
f15333335155515333333b1f351533333515515315556666666666666666ddd1666dddddddddd6664411ddddd111111dd144444fddddddddddddddddf116dddd
f15333351533351533333b1f33533533351535151555566666666666666dddd1666dddddddddd66641dddddddddddddddd144444dddddd1111ddddddf16ddddd
1533333353333353333333b13333515bb1533515f155566666666666666ddd1f6666ddd11ddd666641ddddddddddddddddd11444ddddd144441dddddf1dddddd
1533333333333333335333b1333335b115333353f155566666666666666ddd1f6666dd1001dd666641ddddddddddddddddddd114ddddd14ff41dddddf1dd5555
f15335333333333335153b1f333333b115533333f155566666666666666ddd1f666655100155666641dddddddddddddddddddd14ddddd144441dddddf1d55555
f1535153333333333351b1ff33333515511535331555566666666d66666dddd1666655111155666641dddddddddddddddddddd14dddd14444441ddddf1551111
ff15153333333333333b1fff35335153355331531555666665d666666666ddd16666555555556666441dddddddddddddddddd14fdddd14444441ddddf1511111
fff1533333333533333b1fff515535333333551515556666666666666666ddd16666655555566666f41dddddddddddddddddd14fddd1444114441dddf1111000
fff1533333335153333b1fff351153333333351515556666666666666666ddd16666666666666666f41dddddddddddddddddd14fdddd111dd111ddddf1110000
fff15333333335333351b1ff33551533333351531555566666665666666dddd16666666666666666441dddddddddddddddddd14fddddddddddddddddff110000
ff1515333333333335153b1f3335153333333533f15556666666d666666ddd1f666666666666666641dddddddddddddddddddd14ddddddddddddddddfff11111
f15351533333333333533b1f3333533333333333f155566666666666666ddd1f666666666666666641dddddddddddddddddddd14ddddddddddddddddffffffff
f15335333333333333333b1f3333333333533333f155566666666666666ddd1f66666666666d666641ddddddddddddddddddd114dddddddddddddddd00000000
1533333335333335333333b13333333335153333f1556666666666666666dd1f6666666665566666441dddddddddddddddd11444dddddddddddddddd00000000
1533333351533351533333b1335333533353333315556dddddd66dddddd6ddd166655666d6666666f441dddddddddddddd14444fdddddddddddddddd00000000
f1533333351bbb1533333b1f35153515333335531555ddddddddddddddddddd166d66d6666666666ff441dddddddddddd144444fdddddddddddddddd00000000
f153333333b111b333333b1f5153335335335115155dddddddddddddddddddd166666666666666d6fff4411ddd1111ddd1444fffdddddddddddddddd00000000
ff1bbb33bb1fff1b33bbb1ff353353335153355315ddddddddddddddddddddd16666666666666556fff44441114444111444ffffdddddddddddddddd00000000
fff111bb11fffff1bb111fff3335153335153333fddddd1111dddd1111dddd1f55666666666d5666ffff444444ffff444444ffffdddddddddddddddd00000000
ffffff11ffffffff11ffffff3333533333533333ff1111ffff1111ffff1111ff66d6666d66666666ffffffffffffffffffffffffdddddddddddddddd00000000
00000000222020000011111111111100001dd000000282000000770001400000000004100012022001000010000000000011a861000000000000000010101010
0000000024224200011dddddddddd11001d1110000282820000777700124006006004210012e12ee141111410000000011e1bec1009009000000000151515151
000000022422420011d1111111111d111d11111002828282007777770012441441442100122e11e1124444210000000016e1bec100400400000000115a585651
00020024244342001d111111111111d11d1111102828282807777775140122522522104112ee112241222214001100001111325100444400000001d15b5e5c51
00022024334344201d111111111111d11d1111108282828277777750124111611611142112eee1212444444202ff1000999999990040040000011dcd5b5e5c51
00024244434434201d111111111111d101d1110008282820577775000124441441444210222eee114222222422ff1000541111450024420000161ded5b5e5c51
00224334443434201d111111111111d1001dd00002828200057750000012225225222100222222102411114222220000541111450020020001676ded53525151
02344433344444201d111111111111d100000000002020000055000014111151151111411221110002444420222000005411114500222200156f6d8d55555551
02334444434444201d111111111111d11010101006111600417710211241116116111421222222220d1dd1d006666660444444440020020015666ddd52222251
24434334443344421d111111111111d1010101010061600017777142012444144144421023333332d515515d15666dd549999994002222001555555525555521
23444434444444201d111111111111d1101010100623260077771442001222522522210023333332511111150155ddd549999994001111001999999999999991
02344333444443201d111111111111d101010101623432607774142100141151151141002222222211a9e9110015dd5044444444001001001944444444444491
00234444334432001d111111111111d1101010106333336017114421001241611614210055555555119e8a110001d50055599555001111001999999999999991
012333344333221011d1111111111d11010101016233326001444210000124144142100051111115111111110001500054455445001001001544501010154451
0112222222221110011dddddddddd110101010100623260024422100000012222221000051111115156556510054210054444445001001001544510101054451
00011111111110000011111111111100010101010066600012211000000001111110000051111115011111100542121055555555000000000155101010105510
0020000000000000000000000000001100000000000011110111100000001f100222222222222220000001111100000000000000000000000000000000000000
024202000000200000002200000001410000111000001441144441000001fff10233333333333320001111565111100000066666666600000011111111111100
02442420000220000002341000001410000144410001444101111410001ffff4023333333333332001ddd1d1d1ddd10006666666677666660144444444444410
0244344422242020000244410001410000023441000234110002314101ffff41023333333333332001ddd15651ddd100166666666666666d0144999999994410
2434434442444242002324410014100000232141002321000023214119fff410023333333333332001ddd1d1d1ddd1001666666666666dd10149999999999410
24434344344444420232441002410000023201410232000002320141019f4100023333333333332001ddd15651ddd100156666666666dd100149999999999410
24434444434443202320110023200000232000102320000023200141001910000233333333333320011111d1d111110015566666666dd1000149999999999410
234444434433432032000000320000003200000032000000020000100001000002333333333333201dddd15651dddd100155666666dd10000149999999999410
023444434343322000555000000005000000400005555d5000022200000012200222222222222220155551555155551001556666ddd100000144999999994410
02324443432220000511150000505b50001242205000d6d50123432000012342055555555555555015111111111115101155556dddd400000144444444444410
00202344320000005111115005b5b735012242e2500d676d1234343200123432055555555555555015191a181a1915101445555ddd2410000155559999555510
0000023444200000511111155b73535012282efe5000d6d50123434201234321051000000000015015121812181215101412555dd21441000154445445444510
0000002433200000511111150535b5001288efe250000d051234332112343210051101010101015015115111115115100101255d210144100154445555444510
000012333211000005111115005b735001288e825000000512332232123321000510101010101150155161555161551000001242100014100154444444444510
00012222222210000055115000053500001288205000000501221121012210000511010101010150115515555515511000000141000001000155555555555510
00001111111100000000550000005000000122000555555000110010001100000510101010101150011111111111110000000111000000000011111111111100
ccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccdddddddddccccccccccccccccccccccccccccccccccc
ccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccdddd222222222dddcccccccccccccccccccccccccccccccc
ccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccdd2222111111111222dccccccccccccccccccccccccccccccc
cccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccd2211111000000001112ddccccccccccccccccccccccccccccc
ccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccd2110000000000000000122dccdddccccccccccccccccccccccc
cccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccd21000000000000ddddddd112dd222ddccccccccccccccccccccc
ccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccd2100000000000dd22222222212d11122dcccccccccccccccccccc
ccccccccccccccccccccccccc777cccccccccccccccccccccccccccccccccccccccccccccd2100000000000d22111111111012d01112dccccccccccccccccccc
ccccccccccccccccccccc777777777cccccccccccccccccccccccccccccccccccccccccccd110000000000d2111000000000012d0012ddcccccccccccccccccc
cccccccccccccccccc7777777777777cccccccccccccccccccccccccccccccccccccccccccdd111000000d211000000000000012d00122dccccccccccccccccc
cccccccccccc77777777777777777777777cccccccccccccccccccccccccccccccccccccccccddd10000000000000000000000012d00dd2dddcccccccccccccc
cccccccc7777777777777777777777777777cccccccccccccccccccccccccccccccccccccccccccd111100000000000000000000120d221222dddccccccccccc
ccccccc7777777cccccccccccccccccccccccccccccccccccccccccccccccccccccccccddcccccdddddd1000000000000000000001d2210111222ddccccccccc
ccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccd22dcccd222222d10000000000000000000d211000001112ddcccccccc
ccccccccccccccccccccccccccccccccccccccccccccccccc7cccccccccccccccccccd2112ddd21111112d100000000000000000d210000000000122dccccccc
cccccccccccccccccccccccccccccccccccccccccccc7777777cccccccccccccccccd21001222100000012d10000000000000001d1000000000000112dcccccc
cccccccccccccccccccccbccccccccccccccccc7777777777777777ccccccccccccd2100001110000000012d1000000000000000100000000000000012ddcccc
ccccccccccccccccccbbb3bccccccccccccc77777777777777777777777cccccccd210000000000000000011000000000000000000000000000000000122dccc
ccccccccccccccccccb333bccbbccccccc77777777777777777777777777ccccccd1100000000000000000000000000000000000000000000000000000112dcc
cccccccccccccbbbbc333332cb3bdccccccccccccccccccccccccccccccccccccccd1000000000000000000100000001000000000000000000000000000012dc
c7777ccccccccb333cc3cb34233b5dddcccccccbbbbcccccccccccccccccccccccccd100110000000000001d1000001d100000000000000000000000000001dc
7777777cbbbbc33334bbb34b4235bb55dccccccb333bccccccccccccccccccccccccd110011000000000001d10000001d10000000010000000010000000001dc
c77777cb333bbc332433334bb42b33b55ddcccb333cccccccccccccccccccccccccccdd111110000000001d1d1000001d100000001d11111001d1000000001dc
ccccccbb33333b222b3332233b223335555dbbb242bbbbccccccccccbbbbcccccccccccdddd1100000011d111d11111d1d110000011ddddd11d10000100001dc
ccccbbb3333333c22b33422433b4bb3b5b55b3324b3333bccddddccb33bbbbcccccccccccccd1111111dd1711dddddd1d1dd11111dd111d1dd10000001001dcc
ccccbb3324333342b33444443344b33bb3b553344b333ccdd5555dccc3333bccccccccccccccddddddd1d1711d1d1711d1d1ddddd11d1d1711d110000011dccc
ccccb33342433422b33222444444b33333bb5bbb444cccd5555555dccc2423bccccccccccccc111d1111d171d11d171d11d1d1d11d1d1d171d1dd111111dcccc
cccccc3c2bb4424b33344422bbb2343b4bb3bb33442dcd555555555dc24423bccccccccccccc1d1d1d11d171d11d171d11d1d1d1d11d1d171d111ddddddccccc
cccccdddbb3242522222222b33342423bb3333324425dd5555555555d242ccbccccccccccccc1d1d1d1d1171d1d1171d1d11d1d1d1d11d171d11d1d1d1cccccc
ccccd555d2444252225dd55b32325224b33b52444425d55555555555d242cc3ccccccccccccc1d1d1d1d1711d1d1171d1d1d11d1d1d1d1171d11d1d1d10ccccc
cccf5555f224425555ddddbb244252425243b524242f5555555555555d42ccccccccccccccccc1d11d1d1711d1d1711d1d1d11d1d1d1d1711d1d11d1d10ccccc
dffffffffff2255555d55d535224224224423b52242555555555ffffff2fffddccccccccccccc1d1d11d171d11d1711d1d1d1d11d1d1d1711d1d1d11d10ccccc
1dffffffffffff5ffd5555dfff224f2f242ff3f2f2ffff55555ffffffffffffd11111111111101d1d1d1171d1d1171d11d1d1d1d11d1d171d11d1d11d1011111
11dddddddffffffff555555ffff2fffff2fffffffffffffffffffdddddddddd11111111111101d11d1d1711d1d1171d1d11d1d1d1d11d171d11d1d1d11011111
111111111ddddfffffffffffffffffffffffffffffffffffffffd111111111111111111110001d1d11d1711d1d1711d1d1d11d1d1d1d1171d1d11d1d11001111
1111111111111ddffffffffffffffffffffdddddfffffffddddd1111111111111111111110001d1d1d1171d11d1711d1d1d11d1d1d1d1711d1d1d11d11000111
111111111111111dddddffffffffffffffd11111ddddddd11111111111111111111111111001d11d1d1171d1d1171d11d1d1d11d1d1d1711d1d1100100000111
11111111111111111111dddddddddddddd111111111111111111111111111111111111111001d1d11d1711d1d1171d11d1d1d1d11d1d1711d1d1000000000111
1111111111111111111111111111111111111111111111111111111111111111111111110001d1d1d11711d1d1171d1d11d1d1d1d11d171d1110000000011111
111111111111111111111111111111111111111111111111111111111111111111111110000011d1d1171d11d1711d1d1d11d1d001d1171d1000000001111111
1111111111111111111111101111111111111111111111111111111111111111111111100000001011711d1d1171d11d1d1d10000000011d1000111111111111
1122221111111111222211122221111111222222111111222222211222222221111111100000000000101d1d1171d11101010000000000010011111111111111
12777721111111127777212777721111127777772111127777777227777777721111111000000000000001010171d00000000000000000000111111111111111
27cccc7211111127cccc227cccc7211127cccccc721127ccccccc22cccccccc21111111111000000000000000010100000000000000000001111111111111111
2cc22cc72111127ccccc22cccccc72127cccccccc7212cccccccc22cccccccc21111111111110000000000000000000000000000000000011111111111111111
2cc212cc721127cccccc22cc222cc722ccc2222ccc212ccc2222211222cc22211111111111111000000000000000000000000000000000111111111111111111
2cc2112cc2112cccc22212cc2112cc22cc211112cc212cc21111111112cc21111111111111111000000000000000000000000000000001111111111111111111
2cc2112cc2112ccc211112cc2112cc22cc211112cc212cc21111111112cc21111111111111111000000000000000000000000000000011111111111111111111
2ff2112ff2112ff2111112ff2112ff22ff211112ff212ff21111111112ff21111111111111111111110000000000000000000000000111111111111111111111
2ff2112ff2112ff2111112ff2112ff22ff211112ff212ff21111111112ff21111111111111111111111100000000000000001111111111111111111111111111
2ff2127ff2112ff2111112ff2127ff22ff211112ff212ff21111111112ff21111111111111111111111111100000000000011111111111111111111111111111
2ff227fff2112ff2111112ff227fff22ff222222ff212ff22221111112ff21111111111111111111111111111111111111111111111111111111111111111111
2ff77fff21112ff2111112ff77fff212ff777777ff212ff77772111112ff21111111111111111111111111111111111111111111111111111111111111111111
2ffffff211112ff2111112ffffff2112ffffffffff212ffffff2111112ff21111111111111111111111111111111111111111111111111111111111111111111
2fffff2111112ff2111112ffffff2112ffffffffff212ffffff2111112ff21111111111111111111111111111111111111111111111111111111111111111111
2fff221111112ff2111112ff22ff7212ff222222ff212ff22221111112ff21111111111111111111111111111111111111111111111111111111111111111111
2ff2111111112ff7211112ff222ff722ff211112ff212ff21111111112ff21111111111111111111111111111111111111111111111111111111111111111111
2ff2111112112fff722212ff2112ff22ff211112ff212ff21111111112ff21111111111111111111111111111111111111111111111111111111111111111111
2ff2111127212ffff77722ff2112ff22ff211112ff212ff21111111112ff21111111111111111111111111111111111111111111111111111111111111111111
2ff211127f7212ffffff22ff2112ff22ff211112ff212ff21111111112ff21111111111111111111111111111111111111111111111111111111111111111111
2ff21112fff2112fffff22ff2112ff22ff211112ff212ff21111111112ff21111111111111111111111111111111111111111111111111111111111111111111
122111112f211112ffff22ff2112ff22ff211112ff212ff21111111112ff21111111111111111111111111111111111111111111111111111111111111111111
11111111121111112222112211112211221111112211122111111111112211111111111111111111111111111111111111111111111111111111111111111111
11111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111
__label__
cccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc
cccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc
cccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc
cccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc
cccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc
cccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc
cccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc
cccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc
cccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc
cccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc
cccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc
cccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc
cccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc
cccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc
cccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc
cccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc
cccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc
cccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc
cccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc
cccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc
cccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc
ccccccccccccccccccccccccccccccccccccccccccccccccccccccccc777cccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc
ccccccccccccccccccccccccccccccccccccccccccccccccccccc777777777cccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc
cccccccccccccccccccccccccccccccccccccccccccccccccc7777777777777ccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc
cccccccccccccccccccccccccccccccccccccccccccc77777777777777777777777ccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc
cccccccccccccccccccccccccccccccccccccccc7777777777777777777777777777cccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc
ccccccccccccccccccccccccccccccccccccccc7777777cccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc
cccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc
ccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc7cccccccccccccccccccccccccccccccccccccccccccccc
cccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc7777777ccccccccccccccccccccccccccccccccccccccccccccc
cccccccccccccccccccccccccccccccccccccccccccccccccccccbccccccccccccccccc7777777777777777ccccccccccccccccccccccccccccccccccccccccc
ccccccccccccccccccccccccccccccccccccccccccccccccccbbb3bccccccccccccc77777777777777777777777ccccccccccccccccccccccccccccccccccccc
ccccccccccccccccccccccccccccccccccccccccccccccccccb333bccbbccccccc77777777777777777777777777cccccccccccccccccccccccccccccccccccc
cccccccccccccccccccccccccccccccccccccccccccccbbbbc333332cb3bdccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc
ccccccccccccccccccccccccccccccccc7777ccccccccb333cc3cb34233b5dddcccccccbbbbccccccccccccccccccccccccccccccccccccccccccccccccccccc
cccccccccccccccccccccccccccccccc7777777cbbbbc33334bbb34b4235bb55dccccccb333bcccccccccccccccccccccccccccccccccccccccccccccccccccc
ccccccccccccccccccccccccccccccccc77777cb333bbc332433334bb42b33b55ddcccb333cccccccccccccccccccccccccccccccccccccccccccccccccccccc
ccccccccccccccccccccccccccccccccccccccbb33333b222b3332233b223335555dbbb242bbbbccccccccccbbbbcccccccccccccccccccccccccccccccccccc
ccccccccccccccccccccccccccccccccccccbbb3333333c22b33422433b4bb3b5b55b3324b3333bccddddccb33bbbbcccccccccccccccccccccccccccccccccc
ccccccccccccccccccccccccccccccccccccbb3324333342b33444443344b33bb3b553344b333ccdd5555dccc3333bcccccccccccccccccccccccccccccccccc
ccccccccccccccccccccccccccccccccccccb33342433422b33222444444b33333bb5bbb444cccd5555555dccc2423bccccccccccccccccccccccccccccccccc
cccccccccccccccccccccccccccccccccccccc3c2bb4424b33344422bbb2343b4bb3bb33442dcd555555555dc24423bccccccccccccccccccccccccccccccccc
cccccccccccccccccccccccccccccccccccccdddbb3242522222222b33342423bb3333324425dd5555555555d242ccbccccccccccccccccccccccccccccccccc
ccccccccccccccccccccccccccccccccccccd555d2444252225dd55b32325224b33b52444425d55555555555d242cc3ccccccccccccccccccccccccccccccccc
cccccccccccccccccccccccccccccccccccf5555f224425555ddddbb244252425243b524242f5555555555555d42cccccccccccccccccccccccccccccccccccc
ccccccccccccccccccccccccccccccccdffffffffff2255555d55d535224224224423b52242555555555ffffff2fffddcccccccccccccccccccccccccccccccc
111111111111111111111111111111111dffffffffffff5ffd5555dfff224f2f242ff3f2f2ffff55555ffffffffffffd11111111111111111111111111111111
1111111111111111111111111111111111dddddddffffffff555555ffff2fffff2fffffffffffffffffffdddddddddd111111111111111111111111111111111
11111111111111111111111111111111111111111ddddfffffffffffffffffffffffffffffffffffffffd1111111111111111111111111111111111111111111
111111111111111111111111111111111111111111111ddffffffffffffffffffffdddddfffffffddddd11111111111111111111111111111111111111111111
11111111111111111111111111111111111111111111111dddddffffffffffffffd11111ddddddd1111111111111111111111111111111111111111111111111
1111111111111111111111111111111111111111111111111111dddddddddddddd11111111111111111111111111111111111111111111111111111111111111
11111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111
11111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111
11111111111111111111111111111111111111111111111111111110111111111111111111111111111111111111111111111111111111111111111111111111
11111111111111111111111111111111112222111111111122221112222111111122222211111122222221122222222111111111111111111111111111111111
11111111111111111111111111111111127777211111111277772127777211111277777721111277777772277777777211111111111111111111111111111111
1111111111111111111111111111111127cccc7211111127cccc227cccc7211127cccccc721127ccccccc22cccccccc211111111111111111111111111111111
111111111111111111111111111111112cc22cc72111127ccccc22cccccc72127cccccccc7212cccccccc22cccccccc211111111111111111111111111111111
111111111111111111111111111111112cc212cc721127cccccc22cc222cc722ccc2222ccc212ccc2222211222cc222111111111111111111111111111111111
111111111111111111111111111111112cc2112cc2112cccc22212cc2112cc22cc211112cc212cc21111111112cc211111111111111111111111111111111111
111111111111111111111111111111112cc2112cc2112ccc211112cc2112cc22cc211112cc212cc21111111112cc211111111111111111111111111111111111
111111111111111111111111111111112ff2112ff2112ff2111112ff2112ff22ff211112ff212ff21111111112ff211111111111111111111111111111111111
111111111111111111111111111111112ff2112ff2112ff2111112ff2112ff22ff211112ff212ff21111111112ff211111111111111111111111111111111111
111111111111111111111111111111112ff2127ff2112ff2111112ff2127ff22ff211112ff212ff21111111112ff211111111111111111111111111111111111
111111111111111111111111111111112ff227fff2112ff2111112ff227fff22ff222222ff212ff22221111112ff211111111111111111111111111111111111
111111111111111111111111111111112ff77fff21112ff2111112ff77fff212ff777777ff212ff77772111112ff211111111111111111111111111111111111
111111111111111111111111111111112ffffff211112ff2111112ffffff2112ffffffffff212ffffff2111112ff211111111111111111111111111111111111
111111111111111111111111111111112fffff2111112ff2111112ffffff2112ffffffffff212ffffff2111112ff211111111111111111111111111111111111
111111111111111111111111111111112fff221111112ff2111112ff22ff7212ff222222ff212ff22221111112ff211111111111111111111111111111111111
111111111111111111111111111111112ff2111111112ff7211112ff222ff722ff211112ff212ff21111111112ff211111111111111111111111111111111111
111111111111111111111111111111112ff2111112112fff722212ff2112ff22ff211112ff212ff21111111112ff211111111111111111111111111111111111
111111111111111111111111111111112ff2111127212ffff77722ff2112ff22ff211112ff212ff21111111112ff211111111111111111111111111111111111
111111111111111111111111111111112ff211127f7212ffffff22ff2112ff22ff211112ff212ff21111111112ff211111111111111111111111111111111111
111111111111111111111111111111112ff21112fff2112fffff22ff2112ff22ff211112ff212ff21111111112ff211111111111111111111111111111111111
11111111111111111111111111111111122111112f211112ffff22ff2112ff22ff211112ff212ff21111111112ff211111111111111111111111111111111111
11111111111111111111111111111111111111111211111122221122111122112211111122111221111111111122111111111111111111111111111111111111
11111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111
11111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111
11111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111
11111111111111111111111111111111111111111111111166616161111166116161166166616611111111111111111111111111111111111111111111111111
11111111111111111111111111111111111111111111111161616161111161616161611161616161111111111111111111111111111111111111111111111111
11111111111111111111111111111111111111111111111166116661111161616161666166616161111111111111111111111111111111111111111111111111
11111111111111111111111111111111111111111111111161611161111161616161116161616161111111111111111111111111111111111111111111111111
11111111111111111111111111111111111111111111111166616661111161611661661161616161111111111111111111111111111111111111111111111111
11111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111
11111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111
11111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111
11111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111
11111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111
11111111111111111111111111111111111111111111111111111111666166616611611111111111111111111111111111111111111111111111111111111111
11111111111111111111111111111111111111111111111111111111116161611611611111111111111111111111111111111111111111111111111111111111
11111111111111111111111111111111111111111111111111111111666161611611666111111111111111111111111111111111111111111111111111111111
11111111111111111111111111111111111111111111111111111111611161611611616111111111111111111111111111111111111111111111111111111111
11111111111111111111111111111111111111111111111111111111666166616661666111111111111111111111111111111111111111111111111111111111
11111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111
11111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111
11111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111
11111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111
11111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111
11111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111
11111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111
11111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111
11111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111
11111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111
11111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111
11111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111
11111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111
11111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111
11111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111
11111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111
11111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111
11111111111111111111111111111111111166616661666116611661111166616161666166611661661111116611111111111111111111111111111111111111
11111111111111111111111111111111111161616161611161116111111161616161161116116161616111111611111111111111111111111111111111111111
11111111111111111111111111111111111166616611661166616661111166116161161116116161616111111611111111111111111111111111111111111111
11111111111111111111111111111111111161116161611111611161111161616161161116116161616111111611111111111111111111111111111111111111
11111111111111111111111111111111111161116161666166116611111166611661161116116611616111116661111111111111111111111111111111111111
11111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111
11111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111
11111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111
11111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111
11111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111
11111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111
11111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111
11111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111
11111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111
11111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111
11111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111

__map__
ccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccfccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc4fcccccccccccccccccccccccccccc
ccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccfc44ccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccfcc4cccccccccccccccccccccccccccc
cccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc4fc4cccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc77cccccccccc4fcccccccccccccccccccccccccccccc
cccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc77777777777777777777cccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc7c777777777777777777777777776666cccccc7c7777cc
cccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc77777777777777777777777777776756cccc77777777c7cccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc7c777767767777776776777777777766c5cccccccccccccc
cccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc7c677766667767776676777777776756cccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc77cccccccccccccccccccccccccccccc776677667577666656777777777766c5cccccccccccccccc
cccccccccccccccccccccccccccccccccccccccccc777777777777cccccccccccccccccccccccccc675677567c676666c5776777776766c5cccccccccccccccccccccccccc7777cccccccccccccccccccccc777777777777777777cccccccccccccccccccccccccc67c5675677666656c4776677776666c5cccccccccccccccc
cccccccc77777777cccccccccccccccc777777777777777777777777c7cccccccccccc7c7777c7cc56cc67c5676665f5c4776677666656cccccccccccccccccccccc7c777777777777cccccccccccccccccccccccccc77c7cccccccccccccccccccc7c77777777cccccc66c556555c4fcc676677666656cccccccccccccccccc
cccccccccccccccc7cc7cccccccccccccccccccccccccccccccccccccccccccccc7c777777777777c7cc56ccc5cccc4fcc6c56776666c5ccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc5ccccccfc44cc5c56776656cccccccccccccccccccc
cccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccfccccccccccccccfcc4ccccc57c66c5cccc7777c7cccccccccccccccccccccccccccccccccccccccccccc66666666666666c6ccccccccccccccccccccccccccf2ccccccccccccccfcc4cccccc5c55cc777777777777c7cccccc
cccccccccccccccccccccccccccccccc666677777777776666cccccccccccccccccccccccccc42cfccccccccccccfcc4cccccccccccccccccc77777777c7cccccccccccccccccccccccccccccccccc6c66666677777766666666cccccccccccccccccccccccc42f4ccccccccccccfcc4cc44c4cccccccccccccccccccccccccc
cccccccccccccccccccccccccccccc6656556566777766666666c6cccccccccccccccccccccc4244cfcccccccccc4f242cff44cccccccccccccccccccccccccccccccccccccccccccccccccccccc6c561111116666666666666666cccccccccccccccccccccc2244f4cccccccccc4ff2f2ff4fcccccccccccccccccccccccccc
cccccccccccccccccccccccccccc56151111116666665655556566c6cccccccccccccccccccc224444cfcccccccc4f2cf2ff2fcccccccccccccccccccccccccccccccccc777777cccccccccccc6c5511111161666656111111556566cccccccccccccccccccc224244f4ccccccfc442c22ff22c2cccccccccc3c33cccccccccc
cc7c7777777777cccccccccccc6c1511000060666615111111515565cccccccccccccccccccc2242444fcfccccfc442c222222c2cccccccc3cbcb33bc3cccccccc77777777777777cccccccccc561501000066665611111111515555c5cccccccccccccccccc2222f442f4ccccf7142122222272c7ccccccb3b335b43b3cc3cc
7c777777777777777777cccccc555501606665665611111111515555c5cccccccccccccccccc22222f4444ffffff141122222f7777c7ccc3b3433b553bbb33cccccccccccc7c7777c7cccccccc655665565551665511010000515555c5cccccccccccccccccc2cf24244444f4444f71f111122777777373b4bb3345434b4c3cc
cccccccccccccccccccccccccc65666615115166551100000055555555cccccccccc7777777727242244f44244747c44ffff7177777747343443b335453544cccccccccccccccccccccccccccc55666615115166561500000055555555cccccccc7777777777412222422f747744c77747442f2212d1ff4f4555544555f4ffdf
1111111111111111111111111155555511155166565555005055555555111111111111717711112222f222777777cdcc774424222211ddfdffffffffffdfdd1d11111111111111111111111151555555551551656666555555555555551511111111111111111122222f77c7cc77d7cdcc777777771111d1dddddddddd1d1111
11111111111111111111111175755755551555656666565555555555551511111111111111717727f272c7cccc7c77d7cdcccccccc771711171111111111111111111111111111111111111177755677555555555566555555555555557577111111111171c7cccc2c72cccccccccc7cd7cccccc77cc7c777ccc171111111111
111111111111117717111111d6666d7655555555555555555555555555c7cc1711111171c7cccccccc77ccccccccccccccccccccccccccccccdc111111111111111111111111657777111111dddddd5675575555555555555555555555c7cc17111111c7ccccdddd7dc7ccdcddddddcccccccccccccc7cccd7dd111111111111
1111111111116555551117117117dddd66575555555555555555757777cccc7c111171ccccdc1d11c7ccdc7d7777cccdcc7777c7cccc7cccdd11111111111111111111111111555515617711617711dd6d557755555555557577c7cccccccccc1111ccdcccdd1171ccdc7d77cccccccc77c7ccccccccccdd1d11111111111111
1111117577515555c5656677517617cddc6d765555557775c7cccccccccccccc11d1cccddd1d1171ccdd77ccccccccccccccccc7ccccdc1d111111111111111111117177555c6656cc656676577567d1cddc57557577ccc7ccccccccccccccdc11d1dcdc1d111111dd71c7cccccccccccccc77ccccdcdd111111111111111111
1111775655cc7667cc55666657657715d1cc7d75c7ccccccccccccccccdddd1d1111dddc1111111171c7cccccccccccccc7cccccdcdd1111111111111111111111716755c55c6667d55d656676557615d1ccccc7ccccccccccccccdcdd1111111111111d1111117177ccccccccccdcdd7dc7ccccdd1111111111111111111111
11665655d55d6577565155667657665611cdccccccccccdcddcccc7c11111111111111111111117777ccccccdcdd1d1171ccccdc1d11111111111111111111111166555511516576565155656617655611d1ddcdccccdc1d11ddcdcc171111111111111111117177ccccccccdd11111171cccc1d111111111111111111111111
71555575115155766715556566666566151111d1cccc1d111111d1dd7c17111111111111111171c7ccccccdd11111111c7ccdc1d111111111111111111111111c77757c577115566671651556666656615111111cddd111111111111cddc111111111111111177c7ccccdd111111111171ccdc11111111111111111111111111
cccc77c7cc1d5565765651556666656615111111d111111111111111d11d111111111111111177ccccdd11111117111171ccdc11111111111111111111111111cdcccccccc1d515576665155656655557577171111111111111111111111111111111111117177ccdc1d117177dc1111c7ccdc11111111111111111111111111
cdcc7cd7dd1111556666115555665c55c5cc7c1d111111111111111111111111111111111171c7dc1d1111c7ccdc1111c7ccdd11111111111111111111111111cdcccc7711111155666615555566c577c7cccc1d111111111111111111111111111111111171dd1d111171cccc1d1111c7cc1d11111111111111111111111111
d1cdcccc17111151656616515566d5cdccccdc111111111111111111111111111111111111111d111111c7ccdc111111d1cd1d1111111111111111111111111111d1cccc1d71777765665657555575d1ccdd1d1111111111111111111111111111111111111111111171cccc1d11111111d11d11111111111111111111111111
1111dddd11c7cccc5566565c5575c777d71d111111111111111111111111111111111111111111111171ccdc11111111111111111111111111111111111111111111111177cccccc7c55c5cc7777cccccc1c111111111111111111111111111111111111111111111171ccdc1111111111111111111111111111111111111111
11111171cccc77cccc77c7cccccccccccc1d111111111111111111111111111111111111111111111171cc1d111111111111111111111111111111111111111111111171cc7ccccccccccccccccccccccc1d111111111111111111111111111111111111111111111171cc1d1111111111111111111111111111111111111111
111111c1cccdccdcdd77c7ccccccdcdddd11111111111111111111111111111111111111111111111111dd111111111111111111111111111111111111111111111111d1dcd1cc1d7dccccccccdd1d11111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111
111111111dd1dc11d1ddccccdd1111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111d1111d1dcdd11111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111
111111111111111111111d111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111
__sfx__
000100000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
0128002017164171721d1741a1721d1741c1621a152171720000217172151741a1721717415172191741717217102171721a174151721a17415172191741717200002171721a174171721a1721e1741e17215172
015000200b3520b35511352113550e3520e355153521035217352173551d3521d3551a3521a355213521c3520b3520b35511352113550e3520e3551535210352173551d3521a352213550b352113520e35210355
011400200b1630b3030b16300003116752d6050b1630b1630b163000000b1632d605116752d6252d6252d6250b1630b3030b16300003116752d6050b1630b1630b163000000b1632d605116750b163116750b163
011400201d1741c1621a1521e1741d1741c1621a1521c1621a1521d1741c1621a1521d1741c1621a152171721d104171721515218174151741716215174171620e10217172151521817417162151521716215152
012800200b1550b30511155113050e1550e30515155103050b5750b50511575115050e5750e505155751c3050b1550b30511155113050e1550e30515155105050b5750b50511575115050e5750e5051557510305
012800201d07024070240721d07024070240721d070240702407216070150701607516075160751a0701d0701f0751f075220751a07022070260751d0752b0752a07526075260722607226072260752600526002
0128002023234212312123221235292342a2312a2322a235262342823128232282352d2342e2312e2322e23523234212312123221235292342a2312a2322a235262342823128232282352d2342e2312e2322e235
015000000b1751217211175101720e17516172151750917217575125721d5751c5721a5752257221575155720b1751217211175101720e17516172151750917217575125721d5751c5721a575225722157528572
01140020346451c6751c6551c665346151062510645106351c675106551064510665346151062510635106453464534675346551c665346151c6251c6451c6351c6751c6553464534665346151c6251c63534645
01140020346451c6051c6051c605346151060510605106051c675106051060510605346151060510605106053464534605346051c605346151c6051c6051c6051c6751c6053460534605346151c6051c60534645
00040000066200c62012630156301564012650106500e6500d6500b6500a6500965007650076500c6500f6501065010640106400f6300f6300f6300f6300c6300a62009620096200861003610036100262000000
0001000006620136501a6603f7703f7701867013660106400c6300a62007610056100260002600016000260001600016000260002600026000260002600026000160002600016000160002600036000360001600
000100000000016570255702d57038570385700000000000000001750000000105000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
000100000f7700f7702d6702d67037670346702b6702667026650296502165027650296501e6401c64022630346202e62033630356202e620236302063023630296202f620336103561030610296102760000000
0001000012770127701e670256703067033670346703067027670226602466025660226601f660186601a6601765014650136501164010640106400f6400f6300f6201062010620106100d610106000000000000
0001000019000361303617036170361703616036150361503614036130361103611035110351102e3702e3602e3502e3402e3302e3202e3103310033100331003310032100321003210032100321003210032100
000100001227003270032700327003270032700327003270032700327003270032700327002270022700227003270032600327003260042700426004250042500424005240052300623007220072100000000000
00010000000003a330373603437000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
00010000000001262012620126301263011640106500f6500f6500f6500e6500e6500e6500e6500e6500e6500d6500d6500d6500d6500c6500c6400b6300b6300a6200a6200a6100961009600000000000000000
000100000d4200e4200f4201143013430144401645031450304601c4601e4601f4703947024470294702f4703047027570295702c5702f5203251022570235700000000000000000000000000000000000000000
00020000000000e4700e4701147013470184701b4701d470114701247017470194701c47020470000001c4601f46024460274702947000000204402345027450294702a470014001d4601e46021440274302e420
__music__
00 44024344
01 01020344
00 44020344
02 04050344
01 47084944
01 07080a44
02 07480944

