namespace SmartDesk.Api.Services;

public interface ISentimentService
{
    double Analyze(string message);
}