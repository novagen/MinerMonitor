using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Monitor.Models
{
	public class Miner
	{
		[Key]
		public Guid Id { get; set; }

		[Required]
		[Index(IsUnique = true)]
		public string Name { get; set; }

		[Required]
		public string Url { get; set; }

		[Required]
		public int Port { get; set; }

		public string Username { get; set; }
		public string Password { get; set; }

		[NotMapped]
		public bool Running { get; set; }

		public XmrStakApi.Miner GetStakMiner()
		{
			return new XmrStakApi.Miner(Name, Url, Port, Username, Password);
		}
	}
}