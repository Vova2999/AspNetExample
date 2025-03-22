#pragma warning disable CS8618
// ReSharper disable CollectionNeverUpdated.Global

namespace AspNetExample.Domain.Entities;

public class DoctorExamination
{
	public int Id { get; set; }
	public DateOnly Date { get; set; }
	public int DiseaseId { get; set; }
	public Disease Disease { get; set; }
	public int DoctorId { get; set; }
	public Doctor Doctor { get; set; }
	public int ExaminationId { get; set; }
	public Examination Examination { get; set; }
	public int WardId { get; set; }
	public Ward Ward { get; set; }
}