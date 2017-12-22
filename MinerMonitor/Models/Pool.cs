using NanopoolApi.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static NanopoolApi.Statics;

namespace Monitor.Models
{
	public class Pool
	{
		[Key]
		public Guid Id { get; set; }

		[Required]
		[Index(IsUnique = true)]
		public string Name { get; set; }

		[Required]
		public string Wallet { get; set; }

		[Required]
		public PoolType Type { get; set; }

		[NotMapped]
		public List<Worker> Workers { get; set; }

		public Pool()
		{
			Workers = new List<Worker>();
		}
	}
}