
-- announce
id,title,content,isDel,createTime,updateTime
-- banner
id,pic,jumpUrl,types,isDel,createTime,updateTime
-- shops
id,userId,status,order,title,content,logoPic,lookCount,types,latitude,longitude,location,openTime,closeTime,phoneNum,createTime,
updateTime
beginTime,endTime,shopType
-- shopsDetail
id,shopId,pic,content,isDel,createTime,updateTime
-- messageType
id,title,pic,isDel,types,order,createTime,updateTime
-- messageInfo
id,userId,title,content,lookCount,Pics,order,types,isDel,createTime,updateTime
-- scenic
id,userId,title,lTitle,lookCount,content,mark1,mark2,isDel,order,createTime,updateTime
pic
-- user
id,nickName,phoneNum,passWord,pic,token,uid,status,refId,createTime,updateTime
amount

--order
--wallet



dotnet ef dbcontext scaffold "server=129.28.186.13;port=3306;user id=yoyoba;password=Yoyo123...;database=lfex_service;Charset=utf8mb4;" "Pomelo.EntityFrameworkCore.MySql" -o lfexentitys --context-dir lfexcontext