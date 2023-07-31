using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intel8080Emulator
{
	internal class VideoBuffer
	{
		public Point[,] videoBuffera = new Point[224, 256];
	}

	struct Point
	{
		public int redValue;
		public int greenValue;
		public int blueValue;
	}
}
