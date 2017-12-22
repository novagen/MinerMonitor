using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Monitor.Models
{
	public class Proxy
	{
		//[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		[Key]
		public Guid Id { get; set; }

		[Required]
		[Index(IsUnique = true)]
		public string Url { get; set; }

		[Required]
		public string Username { get; set; }

		[Required]
		public string Password { get; set; }
	}
}