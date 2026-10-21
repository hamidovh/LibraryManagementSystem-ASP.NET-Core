using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Data.Entities;

[Table("tblAuthors")]
[Index("AuthorName", "AuthorSurname", Name = "UQ_tblAuthors_NameSurname", IsUnique = true)]
public partial class TblAuthor
{
    [Key]
    [Column("AuthorID")]
    public int AuthorId { get; set; }

    [StringLength(100)]
    public string AuthorName { get; set; } = null!;

    [StringLength(100)]
    public string AuthorSurname { get; set; } = null!;

    public string? AuthorInfo { get; set; }

    public bool AuthorIsActive { get; set; }

    public DateTime AuthorCreatedDate { get; set; }

    public DateTime? AuthorUpdatedDate { get; set; }

    [InverseProperty("Author")]
    public virtual ICollection<TblAuthorImage> TblAuthorImages { get; set; } = new List<TblAuthorImage>();

    [ForeignKey("AuthorId")]
    [InverseProperty("Authors")]
    public virtual ICollection<TblBook> Books { get; set; } = new List<TblBook>();
}
