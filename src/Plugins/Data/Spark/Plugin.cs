using System.ComponentModel;
using Microsoft.SemanticKernel;
using Microsoft.Spark.Sql;
using Microsoft.Spark.Sql.Streaming;

namespace SKHelpers.Plugins.Spark
{
    public class SparkDataFramePlugin(DataFrame dataFrame)
    {
        [KernelFunction]
        [Description("Writes data to a specified path.")]
        public void WriteData(
            [Description("The path to write data to.")]
            string path,
            [Description("The format of the data (e.g., json, csv).")]
            string format)
            => dataFrame.Write().Format(format).Save(path);

        
        [KernelFunction]
        [Description("Starts a streaming query.")]
        [return: Description("The StreamingQuery object.")]
        public StreamingQuery StartStreamingQueryAsync(
            [Description("The output mode (e.g., append, complete, update).")]
            string outputMode,
            [Description("The stream options")]
            Dictionary<string, string> streamOptions,
            [Description("The output path.")]
            string outputPath)
        {
            var query = dataFrame.WriteStream()
                .OutputMode(outputMode);
            foreach (var option in streamOptions)
            {
                query.Option(option.Key, option.Value);
            }
            return query.Start(outputPath);
        }
    }

    public class StreamingQueryPlugin(StreamingQuery query)
    {
        [KernelFunction]
        [Description("Waits for the query to terminate with a timeout.")]
        public void AwaitTerminationAsync(
            [Description("The timeout in milliseconds.")]
            long timeout)
            => query.AwaitTermination(timeout);


        [KernelFunction]
        [Description("Stops a streaming query.")]
        public void StopStreamingQuery()
            => query.Stop();
    }

    public class SparkSessionPlugin(SparkSession spark)
    {
        [KernelFunction]
        [Description("Reads data from a specified path.")]
        [return: Description("The DataFrame read from the path.")]
        public DataFrame ReadDataAsync(
            [Description("The path to read data from.")]
            string path,
            [Description("The format of the data (e.g., json, csv).")]
            string format)
            => spark.Read().Format(format).Load(path);

        [KernelFunction]
        [Description("Executes a SQL query on the Spark session.")]
        [return: Description("The result of the SQL query.")]
        public DataFrame ExecuteQuery(
            [Description("The SQL query to execute.")]
            string query)
            => spark.Sql(query);
    }
}
