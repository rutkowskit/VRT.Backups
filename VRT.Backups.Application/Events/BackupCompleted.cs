using MediatR;

namespace VRT.Backups.Application.Events;
public sealed record class BackupCompleted(string Message): INotification;
