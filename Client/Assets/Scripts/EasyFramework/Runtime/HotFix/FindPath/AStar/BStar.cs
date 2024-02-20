// namespace Easy
// {

//     using System;
//     using System.Collections.Generic;
//     using System.Linq;

//     /**
//  * A*寻路
//  */

//     public class BStar
//     {
//         /**
//      * 最大寻找次数 
//      */
//         private int _MAX_FIND_NUM = 5000;

//         /**
//      * 直线花费
//      */
//         private int _STRAIGHT_COST = 10;

//         /**
//      * 斜线花费
//      */
//         private int _OBLIQUE_COST = 14;

//         private Dictionary<int, MapNode> _openDic = new Dictionary<int, MapNode>();

//         private Dictionary<int, MapNode> _closeDic = new Dictionary<int, MapNode>();

//         private MapNode _startNode;

//         private MapNode _endNode;

//         private List<MapNode> _endNodes = new List<MapNode>();

//         private int _maxF;

//         private NodeMap _map;

//         public BStar()
//         {

//         }

//         public List<MapNode> FindPath(NodeMap nodeMap, MapNode start, MapNode end, int mapNodeTypes,
//             bool endLinks = true)
//         {
//             this._map = nodeMap;
//             this._map.Refresh();
//             if (!this._map.IsInMap(start.x, start.y) || !this._map.IsInMap(end.x, end.y))
//             {
//                 // DebugerMgr.Instance().Log("AStar-- 在地图外");
//                 return new List<MapNode>();
//             }

//             if (start == end)
//             {
//                 return new List<MapNode>() {start};
//             }

//             this._openDic.Clear();
//             this._closeDic.Clear();
//             this._endNodes.Clear();
//             this._startNode = this._map.GetMapNode(start.x, start.y);
//             if (this._startNode == null || this._startNode.IsObstacle(mapNodeTypes))
//             {
//                 // DebugerMgr.Instance().Log("AStar-- 没有起点或者起点不可走" + (this.startNode == null));
//                 return new List<MapNode>();
//             }

//             this._endNode = this._map.GetMapNode(end.x, end.y);
//             if (this._endNode == null)
//             {
//                 // DebugerMgr.Instance().Log("AStar-- 没有终点");
//                 return new List<MapNode>();
//             }

//             if (this._endNode.IsObstacle(mapNodeTypes))
//             {
//                 if (endLinks)
//                 {
//                     this._endNodes = aStarMap.GetLinkPoints(this._endNode.x, this._endNode.y, mapNodeTypes);
//                 }
//             }
//             else
//             {
//                 this._endNodes.Add(this._endNode);
//             }

//             if (this._endNodes.Count == 0)
//             {
//                 // DebugerMgr.Instance().Log("AStar-- 终点不可到达");
//                 return new List<MapNode>();
//             }

//             this._maxF = this.GetManhattanCost(this._map.GetMapNode(0, 0),
//                 this._map.GetMapNode(this._map.maxX - 1, this._map.maxY - 1));

//             int findNum = 0;
//             MapNode currentNode = this._startNode;

//             while (true)
//             {
//                 ++findNum;
//                 if (findNum >= this._MAX_FIND_NUM)
//                 {
//                     // DebugerMgr.Instance().Log("AStar-- 找太多次了");
//                     return new List<MapNode>();
//                 }

//                 if (currentNode == null)
//                 {
//                     // DebugerMgr.Instance().Log("AStar-- 没用路");
//                     return new List<MapNode>();
//                 }

//                 currentNode = this.GetDirectNode(currentNode);
//                 if (this.IsEndNode(currentNode))
//                 {
//                     return this.BuildPath(currentNode);
//                 }

//                 if(currentNode.IsObstacle(mapNodeTypes))
//                 {

//                 }


//                 this.RemoveFromOpenDic(currentNode);
//                 this.AddCloseDic(currentNode);

//                 List<MapNode> links = this._map.GetLinkPoints(currentNode.x, currentNode.y, mapNodeTypes);
//                 for (int i = 0; i < links.Count(); ++i)
//                 {
//                     MapNode link = links[i];
//                     if (!this.IsInCloseDic(link))
//                     {
//                         int newG = currentNode.g;
//                         if (link.x == currentNode.x || link.y == currentNode.y)
//                         {
//                             newG += this._STRAIGHT_COST * link.mapNodeType;
//                         }
//                         else
//                         {
//                             newG += this._OBLIQUE_COST * link.mapNodeType;
//                         }

//                         if (this.IsInOpenDic(link))
//                         {
//                             if (newG < link.g)
//                             {
//                                 link.g = newG;
//                                 link.h = this.GetManhattanCost(link, this._endNode);
//                                 link.parent = currentNode;
//                             }
//                         }
//                         else
//                         {
//                             link.g = newG;
//                             link.h = this.GetManhattanCost(link, this._endNode);
//                             link.parent = currentNode;
//                             this.AddOpenDic(link);
//                         }
//                     }
//                 }
//             }
//         }

//         public List<MapNode> BuildPath(MapNode endNode)
//         {
//             List<MapNode> path = new List<MapNode>();
//             MapNode tmpNode = endNode;
//             while (tmpNode.parent != null || tmpNode.parent != null)
//             {
//                 path.Add(tmpNode);
//                 tmpNode = tmpNode.parent;
//             }

//             path.Add(tmpNode);
//             path.Reverse();
//             return path;
//         }

//         public MapNode SelectNodeInOpenDic()
//         {
//             MapNode miniFNode = null;
//             foreach (var kv in this._openDic)
//             {
//                 MapNode node = kv.Value;
//                 // DebugerMgr.Instance(DebugerMgr).Log("node == " + (node == null) + " " + key);
//                 if (node == null)
//                     continue;
//                 if (miniFNode == null)
//                 {
//                     miniFNode = node;
//                 }
//                 else if (node.f < miniFNode.f)
//                 {
//                     miniFNode = node;
//                 }
//             }

//             return miniFNode;
//         }

//         public int GetManhattanCost(MapNode node1, MapNode node2)
//         {
//             int dx = System.Math.Abs(node1.x - node2.x);
//             int dy = System.Math.Abs(node1.y - node2.y);
//             return this._STRAIGHT_COST * (dx + dy);
//         }

//         private bool IsStartNode(MapNode node)
//         {
//             return this._startNode.Equals(node);
//         }

//         private bool IsEndNode(MapNode node)
//         {
//             return this._endNodes.Contains(node);
//         }

//         private bool IsInOpenDic(MapNode node)
//         {
//             return this._openDic.ContainsKey(node.key);
//         }

//         private void AddOpenDic(MapNode node)
//         {
//             this._openDic[node.key] = node;
//         }

//         private bool RemoveFromOpenDic(MapNode node)
//         {
//             if (this.IsInOpenDic(node))
//             {
//                 this._openDic.Remove(node.key);
//                 return true;
//             }

//             return false;
//         }

//         private bool IsInCloseDic(MapNode node)
//         {
//             return this._closeDic.ContainsKey(node.key);
//         }

//         private void AddCloseDic(MapNode node)
//         {
//             this._closeDic[node.key] = node;
//         }

//         private void RemoveFromCloseDic(MapNode node)
//         {
//             this._closeDic.Remove(node.key);
//         }

//         public MapNode GetDirectNode(MapNode node)
//         {
//             int x = this._endNode.x - node.x;
//             int y = this._endNode.y - node.y;
//             if(_map.eightDir)
//             {
//                 if(x == 0)
//                 {
//                     return this._map.GetMapNode(node.x, node.y + y / Math.Abs(y));
//                 }
//                 int k = y * 100 / x;
//                 if( k < 150 || k <= -150)
//                 {
//                     return this._map.GetMapNode(node.x, node.y + x / Math.Abs(x));
//                 }
//                 else if( k > 50 && k <= 150)
//                 {
//                     return this._map.GetMapNode(node.x + x / Math.Abs(x), node.y + x / Math.Abs(x));
//                 }
//                 else if( k > -50 && k <= 50)
//                 {
//                     return this._map.GetMapNode(node.x + x / Math.Abs(x), node.y);
//                 }
//                 else if( k > -150 && k <= -50)
//                 {
//                     return this._map.GetMapNode(node.x + x / Math.Abs(x), node.y - x / Math.Abs(x)); 
//                 }
//                 return null;
//             }
//             else
//             {
//                 if(x == 0)
//                 {
//                     return this._map.GetMapNode(node.x, node.y + y / Math.Abs(y));
//                 }
//                 int k = y * 100 / x;
//                 if( k >= -50 && k <= -50)
//                 {
//                     return this._map.GetMapNode(node.x + x / Math.Abs(x) , node.y);
//                 }
//                 else
//                 {
//                     return this._map.GetMapNode(node.x , node.y + y / Math.Abs(y));
//                 }
//                 return null;
//             }
//         }

//         public MapNode ObstaclePath(MapNode node, int mapNodeTypes)
//         {
//             MapNode nextNode = GetDirectNode(node);
//             while(true)
//             {
//                 if(!nextNode.IsObstacle(mapNodeTypes)) 
//                 {
//                     break;
//                 }
//                 nextNode = GetDirectNode(node);
//             }

//             List<MapNode> openDic = new List<MapNode>();
//             List<MapNode> closeDic = new List<MapNode>();

//             openDic.Add(node);
//             openDic.Add(node);

//             int path1Len = 0;
//             MapNode findNode = null;
//             while(openDic.Count != 0)
//             {
//                 findNode = openDic[openDic.Count - 1];
//                 openDic.RemoveAt(openDic.Count - 1);
//                 findNode.g = GetDistanceFromStartAndEnd(findNode);

//                 closeDic.Add(findNode);

//                 if(findNode == node)
//                 {
//                     openDic.RemoveAt(0);
//                 }

//                 if (findNode == nextNode)
//                 {
//                     path1Len = closeDic.Count;
//                 }

//                 MapNode nextFindeNode = null;
//                 List<MapNode> links = _map.GetLinkPoints(findNode.x, findNode.y, int.MaxValue);
//                 for(int i = 0; i < links.Count; ++i)
//                 {
//                     if(links[i] == nextNode)
//                     {
//                         links[i].g = GetDistanceFromStartAndEnd(links[i]);
//                         break;
//                     }

//                     if(links[i].IsObstacle(mapNodeTypes) && !closeDic.Contains(links[i]))
//                     {
//                         List<MapNode> linklinks = _map.GetLinkPoints(links[i].x, links[i].y, mapNodeTypes);
//                         if(linklinks.Count > 0)
//                         {
//                             nextFindeNode = links[i];
//                         }
//                     }

//                     if(nextFindeNode != null)
//                     {
//                         break;
//                     }
//                 }
//                 if(nextFindeNode != null)
//                 {
//                     openDic.Add(nextFindeNode);
//                 }
//             }

//             int pathLen = closeDic.Count;
//             if(path1Len == pathLen)
//             {
                
//             }
            
//         }

//         public int GetDistanceFromStartAndEnd(MapNode node)
//         {
//             int g = GetManhattanCost(_startNode, node);
//             int h = GetManhattanCost(node, _endNode);
//             return g + h;
//         }
//     }
// }

// // https://blog.csdn.net/nanxue1/article/details/124509158

// // #include<bits/stdc++.h>
// // using namespace std;

// // const bool showmap=0;//改为 1 可以逐步查看路径

// // const int SIZE=500;

// // const short direx[]={1,0,-1,0};
// // const short direy[]={0,1,0,-1};

// // struct Point
// // {
// //     int x,y,step;
// //     short WD,D;
// //     const bool operator == (Point ob)
// //     {return x==ob.x && y==ob.y;}
// //     void Scan () {scanf ("%d %d",&x,&y);}
// //     void Print() {printf("%d %d\n",x,y);}
// //     void Walk(short dire)
// //     {x+=direx[dire],y+=direy[dire];}
// //     Point Next(short dire)
// // 	{return Point{x+direx[dire],y+direy[dire],step,WD};}
// // }start,end;

// // int n,m;
// // bool mapn[SIZE+5][SIZE+5],visit[SIZE+5][SIZE+5];
// // queue<Point> B_star;

// // void maprint(Point ob)//展示路径
// // {
// //     for(int i=1;i<=n;i++)
// //     {
// //         for(int j=1;j<=m;j++)
// //             if(mapn[i][j]) cout<<setw(3)<<"#";
// //             else if(Point{j,i}==ob) cout<<setw(3)<<ob.step;
// //             else if(Point{j,i}==end) cout<<setw(3)<<"E";
// //             else if(Point{j,i}==start) cout<<setw(3)<<"S";
// //             else if(visit[i][j]) cout<<setw(3)<<"1";
// //             else cout<<setw(3)<<".";
// //         printf("\n");
// //     }
// // }

// // bool NW(Point ob)//near the wall
// // {
// //     for(short i=0;i<4;i++)
// //     {
// //         Point rear=ob;
// //         rear.Walk(i);
// //         if(mapn[rear.y][rear.x]) return 1;
// //     }
// //     return 0;
// // }

// // bool Allowed(Point ob)
// // {return !mapn[ob.y][ob.x] && !visit[ob.y][ob.x];}

// // bool AtWall(Point ob)
// // {return mapn[ob.y][ob.x];}

// // short SD(Point ob)//straight dire
// // {
// // 	if(abs(ob.x-end.x)>=abs(ob.y-end.y))
// // 	{
// // 		if(ob.x<end.x) return 0;
// // 		if(ob.x>end.x) return 2;
// // 	}
// // 	if(ob.y<end.y) return 1;
// // 	return 3;
// // }

// // int main()
// // {
// //     memset(mapn,1,sizeof mapn);
// //     scanf("%d %d",&n,&m);
// //     for(int i=1;i<=n;i++)
// //         for(int j=1;j<=m;j++)
// //             cin>>mapn[i][j];
// //     start.Scan(),start.step=0,start.WD=start.D=4;
// //     end.Scan();
// //     B_star.push(start);
// //     if(showmap) system("cls");
// //     while(!B_star.empty())
// //     {
// //         Point now=B_star.front();B_star.pop();
// //        	if(now==end) {printf("B-star ans: %d\n",now.step);return 0;}
// //        	if(!Allowed(now)) continue;
// //        	visit[now.y][now.x]=1;
// //        	if(showmap)
// //         {
// //             maprint(now);
// //             system("pause");
// //             system("cls");
// //         }
// //        	/*
// //        		0 右
// // 			1 下
// // 			2 左
// // 			3 上 
// //        	*/
// //        	if(abs(now.x-end.x)>=abs(now.y-end.y))
// //        	{
// //        		if(now.x<end.x && Allowed(now.Next(0)))//朝右走
// //        		{
// //        			Point rear=now.Next(0);
// //        			rear.step++,rear.WD=rear.D=4;
// //        			B_star.push(rear);
// // 				continue;
// // 			}
// // 			if(now.x>end.x && Allowed(now.Next(2)))//朝左走
// // 			{
// // 				Point rear=now.Next(2);
// //        			rear.step++,rear.WD=rear.D=4;
// //        			B_star.push(rear);
// // 				continue;
// // 			}
// // 		}
// // 		else
// // 		{
// // 			if(now.y<end.y && Allowed(now.Next(1)))//朝下走 
// // 	   		{
// // 	   			Point rear=now.Next(1);
// // 	   			rear.step++,rear.WD=rear.D=4;
// // 	   			B_star.push(rear);
// // 				continue;
// // 			}
// // 			if(now.y>end.y && Allowed(now.Next(3)))//朝上走 
// // 			{
// // 				Point rear=now.Next(3);
// // 	   			rear.step++,rear.WD=rear.D=4;
// // 	   			B_star.push(rear);
// // 				continue;
// // 			}
// // 		}
// // 		/*
// //        		0 右
// // 			1 下
// // 			2 左
// // 			3 上 
// //        	*/
// // 		//不能径直走
// // 		 if(now.WD==4 && AtWall(now.Next(SD(now))))//第一次撞到墙2
// // 		 {
// // 			if(SD(now)==0) //墙在右边
// // 			{
// // 				Point rear;
// // 				rear=now.Next(1),rear.D=1,rear.step++,rear.WD=0,B_star.push(rear);
// // 				rear=now.Next(3),rear.D=3,rear.step++,rear.WD=0,B_star.push(rear);
// // 				continue;
// // 			}
// // 			if(SD(now)==1) //墙在下边
// // 			{
// // 				Point rear;
// // 				rear=now.Next(0),rear.D=0,rear.step++,rear.WD=1,B_star.push(rear);
// // 				rear=now.Next(2),rear.D=2,rear.step++,rear.WD=1,B_star.push(rear);
// // 				continue;
// // 			}
// // 			if(SD(now)==2) //墙在左边
// // 			{
// // 				Point rear;
// // 				rear=now.Next(1),rear.D=1,rear.step++,rear.WD=2,B_star.push(rear);
// // 				rear=now.Next(3),rear.D=3,rear.step++,rear.WD=2,B_star.push(rear);
// // 				continue;
// // 			}
// // 			if(SD(now)==3) //墙在上边
// // 			{
// // 				Point rear;
// // 				rear=now.Next(0),rear.D=0,rear.step++,rear.WD=3,B_star.push(rear);
// // 				rear=now.Next(2),rear.D=2,rear.step++,rear.WD=3,B_star.push(rear);
// // 				continue;
// // 			}
// // 		}
// // 		/*
// //        		0 右
// // 			1 下
// // 			2 左
// // 			3 上 
// //        	*/
// // 		else//早就已经撞到墙了
// // 		{
// // 			if(now.WD==0)//墙在右边
// // 			{
// // 				if(!AtWall(now.Next(0)))//右边根本没墙
// // 				{
// // 					if(now.D==1)//向下走
// // 					{
// // 						Point rear;
// // 						rear=now.Next(0),rear.D=0,rear.step++,rear.WD=3,B_star.push(rear);
// // 						continue;
// // 					}
// // 					if(now.D==3)
// // 					{
// // 						Point rear;
// // 						rear=now.Next(0),rear.D=0,rear.step++,rear.WD=1,B_star.push(rear);
// // 						continue;
// // 					}
// // 				}
// // 				//右边有墙，沿着 now.D 继续走
// // 				if(!AtWall(now.Next(now.D)))//能继续走
// // 				{
// // 					Point rear;
// // 					rear=now.Next(now.D),rear.D=now.D,rear.step++,rear.WD=0,B_star.push(rear);
// // 					continue;
// // 				}
// // 				//沿着这个方向不能再走了
// // 			 	Point rear;
// // 				rear=now.Next(2),rear.D=2,rear.step++,rear.WD=now.D,B_star.push(rear);
// // 				continue;
// // 			}
// // 			if(now.WD==1)//墙在下边
// // 			{
// // 				if(!AtWall(now.Next(1)))//下边根本没墙
// // 				{
// // 					if(now.D==0)//向右走
// // 					{
// // 						Point rear;
// // 						rear=now.Next(1),rear.D=1,rear.step++,rear.WD=2,B_star.push(rear);
// // 						continue;
// // 					}
// // 					if(now.D==2)//向左走 
// // 					{
// // 						Point rear;
// // 						rear=now.Next(1),rear.D=1,rear.step++,rear.WD=0,B_star.push(rear);
// // 						continue;
// // 					}
// // 				}
// // 				//下边有墙，沿着 now.D 继续走
// // 				if(!AtWall(now.Next(now.D)))//能继续走
// // 				{
// // 					Point rear;
// // 					rear=now.Next(now.D),rear.D=now.D,rear.step++,rear.WD=1,B_star.push(rear);
// // 					continue;
// // 				}
// // 				//沿着这个方向不能再走了
// // 			 	Point rear;
// // 				rear=now.Next(3),rear.D=3,rear.step++,rear.WD=now.D,B_star.push(rear);
// // 				continue;
// // 			}
// // 			/*
// // 	       		0 右
// // 				1 下
// // 				2 左
// // 				3 上
// //        		*/
// //        		if(now.WD==2)//墙在左边
// // 			{
// // 				if(!AtWall(now.Next(2)))//左边根本没墙
// // 				{
// // 					if(now.D==1)//本来向下走
// // 					{
// // 						Point rear;
// // 						rear=now.Next(2),rear.D=2,rear.step++,rear.WD=3,B_star.push(rear);
// // 						continue;
// // 					}
// // 					if(now.D==3)
// // 					{
// // 						Point rear;
// // 						rear=now.Next(2),rear.D=2,rear.step++,rear.WD=1,B_star.push(rear);
// // 						continue;
// // 					}
// // 				}
// // 				//右边有墙，沿着 now.D 继续走
// // 				if(!AtWall(now.Next(now.D)))//能继续走
// // 				{
// // 					Point rear;
// // 					rear=now.Next(now.D),rear.D=now.D,rear.step++,rear.WD=2,B_star.push(rear);
// // 					continue;
// // 				}
// // 				//沿着这个方向不能再走了
// // 			 	Point rear;
// // 				rear=now.Next(0),rear.D=0,rear.step++,rear.WD=now.D,B_star.push(rear);
// // 				continue;
// // 			}
// // 			if(now.WD==3)//墙在上边
// // 			{
// // 				if(!AtWall(now.Next(3)))//上边根本没墙
// // 				{
// // 					if(now.D==0)//向右走
// // 					{
// // 						Point rear;
// // 						rear=now.Next(3),rear.D=3,rear.step++,rear.WD=2,B_star.push(rear);
// // 						continue;
// // 					}
// // 					if(now.D==2)//向左走 
// // 					{
// // 						Point rear;
// // 						rear=now.Next(3),rear.D=3,rear.step++,rear.WD=0,B_star.push(rear);
// // 						continue;
// // 					}
// // 				}
// // 				//下边有墙，沿着 now.D 继续走
// // 				if(!AtWall(now.Next(now.D)))//能继续走
// // 				{
// // 					Point rear;
// // 					rear=now.Next(now.D),rear.D=now.D,rear.step++,rear.WD=3,B_star.push(rear);
// // 					continue;
// // 				}
// // 				//沿着这个方向不能再走了
// // 			 	Point rear;
// // 				rear=now.Next(1),rear.D=1,rear.step++,rear.WD=now.D,B_star.push(rear);
// // 				continue;
// // 			}
// // 			/*
// // 	       		0 右
// // 				1 下
// // 				2 左
// // 				3 上
// //        		*/
// // 		}
// //     }
// //     printf("No way!\n");
// //     return 0;
// // }
// /*
// 5 5
// 0 0 0 0 0
// 0 0 1 0 0
// 0 0 1 0 0
// 0 0 1 0 0
// 0 0 0 0 0
// 1 3
// 5 3

// 17 17
// 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0
// 0 0 0 0 0 0 0 0 0 0 1 0 0 0 0 0 0
// 0 0 0 0 0 0 0 0 0 0 1 0 0 0 0 0 0
// 0 0 0 0 1 0 0 0 0 0 1 0 0 0 0 0 0
// 0 0 0 0 1 0 0 0 0 0 1 0 0 0 0 0 0
// 0 0 0 0 1 0 0 0 0 0 1 0 0 0 0 0 0
// 0 0 0 0 1 0 0 0 0 0 1 0 0 0 0 0 0
// 0 0 0 0 1 0 0 1 0 0 1 0 0 0 0 0 0
// 0 0 0 0 1 0 0 1 0 0 1 0 0 0 0 0 0
// 0 0 0 0 1 0 0 1 0 0 1 0 0 0 0 0 0
// 0 0 0 0 1 0 0 1 0 0 1 0 0 0 0 0 0
// 0 0 0 0 1 0 0 1 0 0 1 0 0 0 0 0 0
// 0 0 0 0 1 0 0 1 0 0 0 0 0 0 0 0 0
// 0 0 0 0 0 0 0 1 0 0 0 0 0 0 0 0 0
// 0 0 0 0 0 0 0 1 0 0 0 0 0 0 0 0 0
// 0 0 0 0 0 0 0 1 0 0 0 0 0 0 0 0 0
// 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0
// 1 7
// 17 7

// 7 7
// 0 0 0 0 0 0 0
// 0 0 1 1 1 1 0
// 0 0 1 0 0 1 0
// 0 0 1 0 0 1 0
// 0 0 1 0 0 1 0
// 0 0 1 1 1 1 0
// 0 0 0 0 0 0 0
// 1 4
// 5 4

// 5 7
// 0 0 0 0 0 0 0
// 0 0 1 1 1 1 0
// 0 0 0 0 0 1 0
// 0 0 1 1 1 1 0
// 0 0 0 0 0 0 0
// 1 3
// 7 3

// 7 5
// 0 0 0 0 0
// 0 1 1 1 0
// 0 1 0 1 0
// 0 1 0 1 0
// 0 1 0 1 0
// 0 0 0 0 0
// 0 0 0 0 0
// 3 7
// 3 1

// 6 7
// 0 0 0 0 0 0 0
// 0 0 1 1 1 1 0
// 0 0 0 0 0 1 0
// 0 0 0 0 0 1 0
// 0 0 1 1 1 1 0
// 0 0 0 0 0 0 0
// 1 3
// 7 3

// 6 7
// 0 0 0 0 0 0 0
// 0 0 1 1 0 1 1
// 0 0 1 0 0 0 0
// 0 0 1 0 0 0 0
// 0 0 1 1 1 1 1
// 0 0 0 0 0 0 0
// 1 3
// 7 3
// */
