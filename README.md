# Bubble-WHU-SE12


#### 一、项目概述

本项目旨在使用Unity开发一款多人联机的2.5D泡泡堂风格游戏。游戏以经典的泡泡堂玩法为核心，玩家通过控制角色，放置炸弹消除敌人，最终胜出。游戏设计支持多人在线对战，玩家可通过不同的策略和操作，争夺最后的胜利。

#### 二、游戏核心玩法

1. **基本玩法：**
   - 玩家可以控制角色在封闭的地图内移动。
   - 玩家可以放置炸弹，炸弹爆炸后会摧毁障碍物，并对敌人造成伤害。
   - 游戏胜利条件为消灭所有对手，或在规定时间内积分最高的玩家获胜。
2. **游戏模式：**
   - **经典模式**：两组或多人对战，消灭对方所有玩家或最后存活的玩家获胜。
   - **计时模式**：限定时间内，爆炸消除障碍物和敌人，计时结束时积分最高的玩家获胜。
   - **团队模式**：玩家分为两队，协同合作消灭对方团队。
3. **角色与技能：**
   - 每个角色有不同的外观与个性。
   - 玩家可以选择不同的皮肤和道具来增强游戏策略性，如加速、炸弹增强等。



#### 服务端启动方法：

见ConsoleApp1的README.txt



#### 客户端启动方法：

项目直接导入到unity启动即可



#### 注意！！！

1. 务必保证服务端和客户端的Message.cs文件夹一致
2. 服务端转发消息会直接把消息原封不动转发，如果有想写的新消息类型，直接加到Message.cs里即可，不需要改写服务端逻辑。客户端id在SimpleClient类里，连接的时候会自动分配，写新消息类型的时候记得带上id
3. 客户端消息打包见SimpleClient类里的SendMessage和PackAndSend，仿照MoveAction消息逻辑即可。
4. 如果不直接导入项目，而是采取代码的话，记得下载nuget
