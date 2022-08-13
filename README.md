# BilibiliUpdateCheckBot
 哔哩哔哩更新库在Bot的实现
 
## 库
[BilibiliMonitor](https://github.com/Hellobaka/BilibiliMonitor)

## 第三方接口
感谢BiliPlus的海外番剧解析接口

## 指令
* #添加动态uid: 在发送消息的群添加一个动态推送
* #移除动态[uid/列表序号]: 在发送消息的群移除动态推送
* #添加直播uid: 在发送消息的群添加一个直播推送
* #移除直播[uid/列表序号]: 在发送消息的群移除直播推送
* #添加番剧sid：在发送消息的群添加一个番剧推送
* #移除番剧[sid/列表序号]：在发送消息的群移除番剧推送
* #动态列表: 获取发送消息群订阅的动态
* #直播列表: 获取发送消息群订阅的直播
* #番剧列表：获取发送消息群订阅的番剧
* 发送一个B站视频链接将绘制视频基本信息

## 指令参数说明
- 动态：用户UID
- 直播：用户UID
- 番剧：番剧SID

## 番剧SID获取
1. 在哔哩哔哩番剧页的网址截取，比如`https://www.bilibili.com/bangumi/play/ss41417`，`41417`就是sid
2. `https://bangumi.jasonkhew96.eu.org/bilibili` 手动查询，复制`Season ID`

## 视频解析可用模式
- https://www.bilibili.com/video/BVxxx
- https://www.bilibili.com/video/avxxx
- https://b23.tv/avxxx
- https://b23.tv/xxx
- 分享的卡片
- 纯BV/av号

如有其他模式欢迎补充

## 群控制
在数据目录的`Config.json`配置文件内，有三个字段用于控制群启用逻辑，默认情况下启用白名单模式
* Mode: 0=>白名单；1=>黑名单
* WhiteList: 群号数组，在白名单模式下，只有这个数组内的群能够触发功能
* BlackList: 群号数组，在黑名单模式下，只有这个数组内的群不能够触发功能

## 已知问题
- [ ] 部分emoji/特殊字符绘制失败（绘制库的原因，没有办法
- [ ] 内存占用高（500MB+
- [ ] 奇怪的文件夹占用bug
- [ ] 奇怪的什么内存溢出bug
- [x] 动态含gif无法绘制（添加了过滤

## 待更新功能
- [x] 视频解析
- [x] 番剧推送
- [ ] 通过用户名检索
