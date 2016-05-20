namespace DanTup.DaNES.Emulation.Tests.CpuTests
{
	public abstract class CpuTests
	{
		protected Cpu cpu = new Cpu();

		protected void Run(params byte[] program)
		{
			cpu.LoadProgram(program);
			cpu.Run();
		}
	}
}
