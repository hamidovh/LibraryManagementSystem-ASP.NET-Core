using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Data.Entities;

[Table("tblAuthorImages")]
[Index("AuthorId", Name = "IX_tblAuthorImages_AuthorID")]
public partial class TblAuthorImage
{
    [Key]
    [Column("AuthorImageID")]
    public int AuthorImageId { get; set; }

    [Column("AuthorID")]
    public int AuthorId { get; set; }

    [StringLength(500)]
    public string ImagePath { get; set; } = null!;

    public int ImageSortOrder { get; set; }

    [ForeignKey("AuthorId")]
    [InverseProperty("TblAuthorImages")]
    public virtual TblAuthor Author { get; set; } = null!;
}
