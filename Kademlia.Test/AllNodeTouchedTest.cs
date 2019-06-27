using System;
using System.Collections.Generic;
using Kademlia;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Kademlia.Test
{
	[TestClass]
	public class AllNodeTouchedTest
	{
		private List<KademliaNode> _kademliaNodes;
		private int NODECOUNT = 1 << 12;

		[TestMethod]
		public void SendOneBroadCast()
		{
			_kademliaNodes = new List<KademliaNode>();

			for (int i = 0; i < NODECOUNT; i++)
				CreateKademliaNode(i);

			BroadCastPingAllNodes(_kademliaNodes, NODECOUNT);

			Random r = new Random();
			int randNum = r.Next(NODECOUNT);
			_kademliaNodes[randNum].BroadCast("msg");

			Assert.IsTrue(AllKademliaNodeTouched());
		}

		//[TestMethod]
		public void SendAllBroadCasts()
		{
			_kademliaNodes = new List<KademliaNode>();

			for (int i = 0; i < NODECOUNT; i++)
				CreateKademliaNode(i);

			_kademliaNodes[0].PingAll();

			for (int i = 0; i < NODECOUNT; i++)
				_kademliaNodes[i].BroadCast("msg");

			Assert.IsTrue(AllKademliaNodeTouched());
		}

		private bool CreateKademliaNode(int id)
		{
			foreach (KademliaNode node in _kademliaNodes)
			{
				if (node.GetId() == id)
					return false;
			}

			KademliaNode newNode = new KademliaNode(id);
			if (id != 0)
				newNode.Bootstrap(_kademliaNodes[0]);
			_kademliaNodes.Add(newNode);
			return true;
		}

		private bool AllKademliaNodeTouched()
		{
			bool success = true;
			foreach (KademliaNode node in _kademliaNodes)
			{
				if (!node.Touched())
				{
					success = false;
				}
			}

			return success;
		}

		private static void BroadCastPingAllNodes(List<KademliaNode> nodes, int size)
		{
			for (int i = 0; i < size; i++)
				nodes[i].PingAll();
		}
	}
}
