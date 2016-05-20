namespace DanTup.DaNES.Emulation.Tests.CpuTests
{
	abstract class CpuTests
	{
		protected Nes nes = new Nes();

		protected void Run(params byte[] program)
		{
			nes.LoadProgram(program, false);
			nes.Run();
		}
	}
}
