using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistributedTasksOnTime.BlazorComponent;

public class ProgressMeter
{
	public int Min { get; set; }
	public int Max { get; set; }
	public int? Low { get; set; }
	public int? High { get; set; }
	public int Value { get; set; }
	public int? Optimum { get; set; }
	public string Content { get; set; }
}