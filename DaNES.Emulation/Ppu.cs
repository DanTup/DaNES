namespace DanTup.DaNES.Emulation
{
	class Ppu
	{
		public Memory Ram { get; }

		public Ppu(Memory ram)
		{
			Ram = ram;
		}
	}
}
