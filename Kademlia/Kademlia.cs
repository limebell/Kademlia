﻿using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kademlia
{
	public class Kademlia
	{
		private const int MAX_TRIES = 1 << 10;
		static private List<NormalNode> _normalNodes;
		static private List<KademliaNode> _kademliaNodes;
		static private long tnormalbu, tkademliabu, tnormalbr, tkademliabr;
		static private int utotalN, utotalK, dtotalN, dtotalK, muploadsN, muploadsK, mdownloadsN, mdownloadsK;
		static private Random r;
		static void Main(string[] args)
		{
			int NODECOUNT = 1 << 10;

			_normalNodes = new List<NormalNode>();
			_kademliaNodes = new List<KademliaNode>();

			r = new Random();

			Stopwatch sw = Stopwatch.StartNew();
			tnormalbu = sw.ElapsedTicks;
			/*for (int i = 0; i < NODECOUNT; i++)
				CreateNormalNode(i);*/
			tnormalbu = sw.ElapsedTicks - tnormalbu;
			tkademliabu = sw.ElapsedTicks;
			CreateKademliaNode(0);
			List<int> randomList = GenerateRandomList(NODECOUNT);
			for (int i = 0; i < NODECOUNT; i++)
			{
				CreateKademliaNode(randomList[i]);
			}
			tkademliabu = sw.ElapsedTicks - tkademliabu;
			BroadCastPingAllNodes(NODECOUNT);

			/*for (int i = 0; i < 100; i++)
			{
				HaltRandomNode(NODECOUNT, haltCount: 1, trace: false);
			}*/
			//CloseRandomNode(NODECOUNT, closeCount: 2000, trace: false);

			//DisplayAllTable();

			SendOneBroadCast(NODECOUNT, target: -1, trace: true);
			//SendMultipleBroadCasts(NODECOUNT, 2000);
			//SendAllBroadCasts(NODECOUNT);

			if (AllKademliaNodeTouched(true))
				Console.WriteLine("All Kademlia node touched");

			/*Console.WriteLine("=============================STATISTICS=============================");
			Console.WriteLine("\t\t\t\tNormal\t\tKademlia");
			Console.WriteLine("Elapsed Ticks For Building\t" + tnormalbu + "\t\t" + tkademliabu);
			Console.WriteLine("Elapsed Ticks Per Broadcast\t" + tnormalbr + "\t\t" + tkademliabr);
			Console.WriteLine("Total Uploads\t\t\t" + utotalN + "\t\t" + utotalK);
			Console.WriteLine("Total Downloads\t\t\t" + dtotalN + "\t\t" + dtotalK);
			Console.WriteLine("Maximum Uploads\t\t\t" + muploadsN + "\t\t" + muploadsK);
			Console.WriteLine("Maximum Downloads\t\t" + mdownloadsN + "\t\t" + mdownloadsK);*/
		}

		static public List<int> GenerateRandomList(int size)
		{
			List<int> ret = new List<int>();
			for (int i = 0; i < size; i++)
				ret.Add(i);

			Random nr = new Random();

			return ret.OrderBy(val => nr.NextDouble()).ToList();
		}

		static public void SendOneBroadCast(int size, int target = -1, bool trace = false)
		{
			Console.WriteLine("\n***SendOneBroadCast***");

			int randNum = target == -1 ? r.Next(size) : target;
			int tries;
			for (tries = 0; tries < MAX_TRIES; tries++)
			{
				if (!_kademliaNodes[randNum].Respond())
				{
					if (target == -1)
						randNum = r.Next(size);
					else
					{
						if (trace) Console.WriteLine("Node index {0}, id {1} is not alive.", target, _kademliaNodes[target].GetId());
						return;
					}
				}
				else
					break;
			}

			if (tries >= MAX_TRIES)
			{
				if (trace) Console.WriteLine("Broadcast failed... cannot find alive node.");
				return;
			}

			if(trace) Console.WriteLine("Broadcasting to index {0}, id {1} with {2} tries.", randNum, _kademliaNodes[randNum].GetId(), tries + 1);
			Stopwatch sw = Stopwatch.StartNew();
			//_normalNodes[randNum].BroadCast("msg");
			tnormalbr = sw.ElapsedTicks;
			_kademliaNodes[randNum].BroadCast("msg");
			tkademliabr = sw.ElapsedTicks - tnormalbr;

			if (trace)
			{
				Console.WriteLine("=========Log=========");
				_kademliaNodes[randNum].PrintLog();
				Console.WriteLine();
			}

			utotalN = utotalK = dtotalN = dtotalK = muploadsN = muploadsK = mdownloadsN = mdownloadsK = 0;
			for (int i = 0; i < size; i++)
			{
				int temp;
				/*temp = _normalNodes[i].GetUploads();
				if (temp > muploadsN) muploadsN = temp;
				utotalN += temp;
				temp = _normalNodes[i].GetDownloads();
				if (temp > mdownloadsN) mdownloadsN = temp;
				dtotalN += temp;*/
				temp = _kademliaNodes[i].GetUploads();
				if (temp > muploadsK) muploadsK = temp;
				utotalK += temp;
				temp = _kademliaNodes[i].GetDownloads();
				if (temp > mdownloadsK) mdownloadsK = temp;
				dtotalK += temp;
			}
		}


		static public void SendMultipleBroadCasts(int size, int sendCount, bool trace = false)
		{
			Console.WriteLine("***SendMultipleBroadCasts***");
			Stopwatch sw = Stopwatch.StartNew();
			long elapsedTime = sw.ElapsedTicks;
			for (int i = 0; i < sendCount; i++)
			{
				int randNum = r.Next(size);
				if (trace) Console.WriteLine("Broadcasting to index " + randNum);
				_normalNodes[randNum].BroadCast("msg");
				elapsedTime = sw.ElapsedTicks - elapsedTime;
				tnormalbr = elapsedTime > tnormalbr ? elapsedTime : tnormalbr;
				_kademliaNodes[randNum].BroadCast("msg");
				elapsedTime = sw.ElapsedTicks - elapsedTime;
				tkademliabr = elapsedTime > tkademliabr ? elapsedTime : tkademliabr;
			}

			utotalN = utotalK = dtotalN = dtotalK = muploadsN = muploadsK = mdownloadsN = mdownloadsK = 0;
			for (int i = 0; i < size; i++)
			{
				int temp;
				temp = _normalNodes[i].GetUploads();
				if (temp > muploadsN) muploadsN = temp;
				utotalN += temp;
				temp = _normalNodes[i].GetDownloads();
				if (temp > mdownloadsN) mdownloadsN = temp;
				dtotalN += temp;
				temp = _kademliaNodes[i].GetUploads();
				if (temp > muploadsK) muploadsK = temp;
				utotalK += temp;
				temp = _kademliaNodes[i].GetDownloads();
				if (temp > mdownloadsK) mdownloadsK = temp;
				dtotalK += temp;
			}
		}

		static public void SendAllBroadCasts(int size, bool trace = false)
		{
			Console.WriteLine("***SendAllBroadCasts***");
			Stopwatch sw = Stopwatch.StartNew();
			long elapsedTime = sw.ElapsedTicks;
			for (int i = 0; i < size; i++)
			{
				if (trace) Console.WriteLine("Broadcasting to index " + i);
				_normalNodes[i].BroadCast("msg");
				elapsedTime = sw.ElapsedTicks - elapsedTime;
				tnormalbr = elapsedTime > tnormalbr ? elapsedTime : tnormalbr;
				_kademliaNodes[i].BroadCast("msg");
				elapsedTime = sw.ElapsedTicks - elapsedTime;
				tkademliabr = elapsedTime > tkademliabr ? elapsedTime : tkademliabr;
			}

			utotalN = utotalK = dtotalN = dtotalK = muploadsN = muploadsK = mdownloadsN = mdownloadsK = 0;
			for (int i = 0; i < size; i++)
			{
				int temp;
				temp = _normalNodes[i].GetUploads();
				if (temp > muploadsN) muploadsN = temp;
				utotalN += temp;
				temp = _normalNodes[i].GetDownloads();
				if (temp > mdownloadsN) mdownloadsN = temp;
				dtotalN += temp;
				temp = _kademliaNodes[i].GetUploads();
				if (temp > muploadsK) muploadsK = temp;
				utotalK += temp;
				temp = _kademliaNodes[i].GetDownloads();
				if (temp > mdownloadsK) mdownloadsK = temp;
				dtotalK += temp;
			}
		}

		static public bool CreateNormalNode(int id)
		{
			foreach (NormalNode node in _normalNodes)
			{
				if (node.GetId() == id)
					return false;
			}

			NormalNode newNode = new NormalNode(id, _normalNodes);
			foreach (NormalNode node in _normalNodes)
				node.NewNode(newNode);
			_normalNodes.Add(newNode);
			return true;
		}

		static public bool CreateKademliaNode(int id)
		{
			foreach (KademliaNode node in _kademliaNodes)
			{
				if (node.GetId() == id)
					return false;
			}

			KademliaNode newNode = new KademliaNode(id);
			if(id != 0)
				newNode.Bootstrap(_kademliaNodes[0]);
			_kademliaNodes.Add(newNode);
			return true;
		}

		static public void DisplayAllTable()
		{
			int index = 0;
			foreach (KademliaNode node in _kademliaNodes)
			{
				Console.WriteLine("Node index {0}, id {1}", index, node.GetId());
				if (node.Respond())
					Console.WriteLine(node.PrintTable());
				else
					Console.WriteLine("Dead");
				Console.WriteLine();
				index++;
			}
		}

		static public bool AllKademliaNodeTouched(bool trace = false)
		{
			int index = 0;
			bool success = true;
			foreach (KademliaNode node in _kademliaNodes)
			{
				if (node.Respond() && !node.Touched())
				{
					if(trace) Console.WriteLine("Node of index {0}, id {1} is not touched", index, node.GetId());
					success = false;
				}
				index++;
			}

			return success;
		}

		static public bool AllNormalNodeTouched(bool trace = false)
		{
			bool success = true;
			foreach (NormalNode node in _normalNodes)
			{
				if (!node.Touched())
				{
					if(trace) Console.WriteLine("Node of id " + node.GetId() + " is not touched");
					success = false;
				}
			}

			return success;
		}

		static public void CloseRandomNode(int size, int closeCount = 1, bool trace = false)
		{
			for (int i = 0; i < closeCount; i++)
			{
				int randNum = r.Next(size);
				if (!_kademliaNodes[randNum].Respond())
				{
					if (trace) Console.WriteLine("Node at {0} already closed", randNum);
					continue;
				}

				if (trace) Console.WriteLine("Closed node of index " + randNum);
				_kademliaNodes[randNum].Close();
			}
		}

		static public void HaltRandomNode(int size, int haltCount = 1, bool trace = false)
		{
			for (int i = 0; i < haltCount; i++)
			{
				int randNum = r.Next(size);
				if (!_kademliaNodes[randNum].Respond())
				{
					if (trace) Console.WriteLine("Node at index {0} already halted", randNum);
					continue;
				}

				if (trace) Console.WriteLine("Halt node of index {0}, id {1}", randNum, _kademliaNodes[randNum].GetId());
				_kademliaNodes[randNum].Halt();
			}
			for (int i = 0; i < 1; i++) PingAllKademliaNodes(size);
		}

		public static void BroadCastPingAllNodes(int size)
		{
			for (int i = 0; i < size; i++)
			{
				_kademliaNodes[i].PingAll();
			}
		}

		public static void PingAllKademliaNodes(int size)
		{
			for (int i = 0; i < size; i++)
			{
				_kademliaNodes[i].PingTable();
			}
		}
	}
}
