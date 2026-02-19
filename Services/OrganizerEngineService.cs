using System.IO.Compression;
using TheWatcher.Data.Models;
using TheWatcher.Interfaces;

namespace TheWatcher.Services
{
    public class OrganizerEngineService : IOrganizerEngine
    {
        public FileTask ProcessFile(FileTask task, List<OrganizerRuler> rules)
        {
            task.Status = Data.Models.TaskStatus.Pending;
            task.MatchedRule = null;

            foreach (var rule in rules)
            {
                if (DoesRuleMatch(task, rule))
                {
                    task.MatchedRule = rule;
                    task.ProposedNewPath = CalculateDestination(task, rule);
                    task.Status = Data.Models.TaskStatus.Ready;

                    return task;
                }
            }

            return task;
        }

        private bool DoesRuleMatch(FileTask task, OrganizerRuler rule)
        {
            if (rule.Conditions == null || !rule.Conditions.Any()) return false;

            foreach (var condition in rule.Conditions)
            {
                bool isMatch = EvaluateCondition(task, condition);
                if (!isMatch) return false;
            }

            return true;
        }

        private bool EvaluateCondition(FileTask task, RuleCondition condition)
        {
            try
            {
                string fileValue = string.Empty;
                string ruleValue = condition.Value?.ToLower().Trim() ?? "";

                switch (condition.Field)
                {
                    case RuleCondition.ConditionType.Extension:
                        fileValue = Path.GetExtension(task.OriginalFullPath).ToLower();
                        break;

                    case RuleCondition.ConditionType.FileName:
                        fileValue = task.FileName.ToLower();
                        break;

                    case RuleCondition.ConditionType.Size:
                        long fileSize = task.SizeBytez;
                        long ruleSize = long.Parse(ruleValue);
                        return CompareNumbers(fileSize, ruleSize, condition.Operator);

                    case RuleCondition.ConditionType.DateCreated:
                        return true;
                }

                return CompareStrings(fileValue, ruleValue, condition.Operator);
            }
            catch
            {
                return false;
            }
        }

        public async Task<FileExecutionLog> ExecuteTaskAsync(FileTask task)
        {
            var log = new FileExecutionLog
            {
                OriginalFilePath = task.OriginalFullPath,
                Timestamp = DateTime.Now,
                RuleName = task.MatchedRule?.Name ?? "Manual"
            };

            try
            {
                if (task.MatchedRule == null)
                    throw new Exception("No rule matched.");

                string destDir = task.MatchedRule.DestinationDirectory ?? string.Empty;

                if (!Directory.Exists(destDir))
                {
                    Directory.CreateDirectory(destDir);
                }

                switch (task.MatchedRule.ActionType)
                {
                    case RuleActionType.Move:
                        try
                        {
                            if (File.Exists(task.ProposedNewPath))
                            {
                                log.Status = LogStatus.Skipped;
                                log.ErrorMessage = "Destination file already exists.";
                            }
                            else
                            {
                                File.Move(task.OriginalFullPath, task.ProposedNewPath);
                                log.Status = LogStatus.Success;
                                log.NewFilePath = task.ProposedNewPath;
                            }
                        }
                        catch (IOException ex)
                        {
                            log.Status = LogStatus.Failed;
                            log.ErrorMessage = "File is in use or locked: " + ex.Message;
                        }
                        catch (UnauthorizedAccessException ex)
                        {
                            log.Status = LogStatus.Failed;
                            log.ErrorMessage = "Access denied (Check permissions or ReadOnly): " + ex.Message;
                        }
                        catch (Exception e)
                        {
                            log.Status = LogStatus.Failed;
                            log.ErrorMessage = "Move failed: " + e.Message;
                        }
                        break;

                    case RuleActionType.Copy:
                        try
                        {
                            File.Copy(task.OriginalFullPath, task.ProposedNewPath, overwrite: false);
                            log.Status = LogStatus.Success;
                        }
                        catch (IOException ex)
                        {
                            log.Status = LogStatus.Failed;
                            log.ErrorMessage = "File is in use or locked: " + ex.Message;
                        }
                        catch (UnauthorizedAccessException ex)
                        {
                            log.Status = LogStatus.Failed;
                            log.ErrorMessage = "Access denied (Check permissions or ReadOnly): " + ex.Message;
                        }
                        catch (Exception e)
                        {
                            log.Status = LogStatus.Failed;
                            log.ErrorMessage = "Copy failed: " + e.Message;
                        }
                        break;

                    case RuleActionType.Delete:
                        try
                        {
                            File.Delete(task.OriginalFullPath);
                            log.Status = LogStatus.Success;
                        }
                        catch (IOException ex)
                        {
                            log.Status = LogStatus.Failed;
                            log.ErrorMessage = "File is in use or locked: " + ex.Message;
                        }
                        catch (UnauthorizedAccessException ex)
                        {
                            log.Status = LogStatus.Failed;
                            log.ErrorMessage = "Access denied (Check permissions or ReadOnly): " + ex.Message;
                        }
                        catch (Exception e)
                        {
                            log.Status = LogStatus.Failed;
                            log.ErrorMessage = "Delete failed: " + e.Message;
                        }

                        break;
                    case RuleActionType.Rename:
                        try
                        {
                            if (File.Exists(task.ProposedNewPath))
                            {
                                log.Status = LogStatus.Skipped;
                                log.ErrorMessage = "A file with this name already exist";
                            }
                            else
                            {
                                File.Move(task.OriginalFullPath, task.ProposedNewPath);

                                log.Status = LogStatus.Success;
                                log.NewFilePath = task.ProposedNewPath;
                            }
                        }
                        catch (IOException ex)
                        {
                            log.Status = LogStatus.Failed;
                            log.ErrorMessage = "File is in use or locked: " + ex.Message;
                        }
                        catch (UnauthorizedAccessException ex)
                        {
                            log.Status = LogStatus.Failed;
                            log.ErrorMessage = "Access denied (Check permissions or ReadOnly): " + ex.Message;
                        }
                        catch (Exception e)
                        {
                            log.Status = LogStatus.Failed;
                            log.ErrorMessage = "Rename failed: " + e.Message;
                        }
                        break;

                    case RuleActionType.Archive:

                        try
                        {

                            bool isMasterArchive = destDir.EndsWith(".zip", StringComparison.OrdinalIgnoreCase);

                            string finalZipPath;

                            if (isMasterArchive)
                            {
                                finalZipPath = destDir;

                                Directory.CreateDirectory(Path.GetDirectoryName(finalZipPath)!);

                                lock (_zipLock)
                                {
                                    PerformZip(finalZipPath, task.OriginalFullPath);
                                }
                            }
                            else
                            {
                                finalZipPath = Path.ChangeExtension(task.ProposedNewPath, ".zip");

                                Directory.CreateDirectory(Path.GetDirectoryName(finalZipPath)!);

                                PerformZip(finalZipPath, task.OriginalFullPath);
                            }

                            File.Delete(task.OriginalFullPath);

                            log.Status = LogStatus.Success;
                            log.NewFilePath = finalZipPath;

                            void PerformZip(string zipPath, string sourceFilePath)
                            {
                                using (ZipArchive archive = ZipFile.Open(zipPath, ZipArchiveMode.Update))
                                {
                                    string entryName = Path.GetFileName(sourceFilePath);

                                    if (archive.GetEntry(entryName) != null)
                                    {
                                        string nameNoExt = Path.GetFileNameWithoutExtension(entryName);
                                        string ext = Path.GetExtension(entryName);
                                        entryName = $"{nameNoExt}_{DateTime.Now.Ticks}{ext}";
                                    }

                                    archive.CreateEntryFromFile(sourceFilePath, entryName);
                                }
                            }

                        }

                        catch (IOException ex)
                        {
                            log.Status = LogStatus.Failed;
                            log.ErrorMessage = "File is in use or locked: " + ex.Message;
                        }
                        catch (UnauthorizedAccessException ex)
                        {
                            log.Status = LogStatus.Failed;
                            log.ErrorMessage = "Access denied (Check permissions or ReadOnly): " + ex.Message;
                        }
                        catch (Exception e)
                        {
                            log.Status = LogStatus.Failed;
                            log.ErrorMessage = "Rename failed: " + e.Message;
                        }
                        break;
                }

            }
            catch (Exception e)
            {
                log.ErrorMessage = e.Message;
            }

            return log;
        }

        // handlers
        private bool CompareStrings(string fileVal, string ruleVal, RuleCondition.OperatorType op)
        {
            return op switch
            {
                RuleCondition.OperatorType.Equals => fileVal == ruleVal,
                RuleCondition.OperatorType.NotEquals => fileVal != ruleVal,
                RuleCondition.OperatorType.Contains => fileVal.Contains(ruleVal),
                RuleCondition.OperatorType.StartsWith => fileVal.StartsWith(ruleVal),
                RuleCondition.OperatorType.EndsWith => fileVal.EndsWith(ruleVal),
                _ => false
            };
        }

        private bool CompareNumbers(long fileVal, long ruleVal, RuleCondition.OperatorType op)
        {
            return op switch
            {
                RuleCondition.OperatorType.Equals => fileVal == ruleVal,
                RuleCondition.OperatorType.GreaterThan => fileVal > ruleVal,
                RuleCondition.OperatorType.LessThan => fileVal < ruleVal,
                _ => false
            };
        }
        private string CalculateDestination(FileTask task, OrganizerRuler rule)
        {
            if (string.IsNullOrEmpty(rule.DestinationDirectory)) return string.Empty;

            return Path.Combine(rule.DestinationDirectory, task.FileName);
        }

        private static readonly object _zipLock = new object();

    }
}
