using System.Collections.Generic;

namespace PlayverseMetrics.Models.AWS
{
    public class AWSSummary
    {
        public Data data { get; set; }
        public string groupBy { get; set; }
        public List<int> hours { get; set; }
        public int interval { get; set; }
        public long start { get; set; }
        public Stats stats { get; set; }
        public int status { get; set; }
        public List<long> time { get; set; }
    }

    public class Data
    {
        public List<double> AWSKeyManagementService { get; set; }
        public List<double> AWSLambda { get; set; }
        public List<double> AmazonCloudWatch { get; set; }
        public List<double> aggregated { get; set; }
        public List<double> cloudfront { get; set; }
        public List<double> cloudwatch { get; set; }
        public List<double> dynamodb { get; set; }
        public List<double> ebs { get; set; }
        public List<double> ec2 { get; set; }
        public List<double> ec2_instance { get; set; }
        public List<double> eip { get; set; }
        public List<double> elasticache { get; set; }
        public List<double> rds { get; set; }
        public List<double> route53 { get; set; }
        public List<double> s3 { get; set; }
        public List<double> ses { get; set; }
        public List<double> simpledb { get; set; }
        public List<double> sns { get; set; }
        public List<double> sqs { get; set; }
        public List<double> vpc { get; set; }
    }

    public class AWSKeyManagementService
    {
        public double average { get; set; }
        public double max { get; set; }
        public double total { get; set; }
    }

    public class AWSLambda
    {
        public double average { get; set; }
        public double max { get; set; }
        public double total { get; set; }
    }

    public class AmazonCloudWatch
    {
        public double average { get; set; }
        public double max { get; set; }
        public double total { get; set; }
    }

    public class Aggregated
    {
        public double average { get; set; }
        public double max { get; set; }
        public double total { get; set; }
    }

    public class Cloudfront
    {
        public double average { get; set; }
        public double max { get; set; }
        public double total { get; set; }
    }

    public class Cloudwatch
    {
        public double average { get; set; }
        public double max { get; set; }
        public double total { get; set; }
    }

    public class Dynamodb
    {
        public double average { get; set; }
        public double max { get; set; }
        public double total { get; set; }
    }

    public class Ebs
    {
        public double average { get; set; }
        public double max { get; set; }
        public double total { get; set; }
    }

    public class Ec2
    {
        public double average { get; set; }
        public double max { get; set; }
        public double total { get; set; }
    }

    public class Ec2Instance
    {
        public double average { get; set; }
        public double max { get; set; }
        public double total { get; set; }
    }

    public class Eip
    {
        public double average { get; set; }
        public double max { get; set; }
        public double total { get; set; }
    }

    public class Elasticache
    {
        public double average { get; set; }
        public double max { get; set; }
        public double total { get; set; }
    }

    public class Rds
    {
        public double average { get; set; }
        public double max { get; set; }
        public double total { get; set; }
    }

    public class Route53
    {
        public double average { get; set; }
        public double max { get; set; }
        public double total { get; set; }
    }

    public class S3
    {
        public double average { get; set; }
        public double max { get; set; }
        public double total { get; set; }
    }

    public class Ses
    {
        public double average { get; set; }
        public double max { get; set; }
        public double total { get; set; }
    }

    public class Simpledb
    {
        public double average { get; set; }
        public double max { get; set; }
        public double total { get; set; }
    }

    public class Sns
    {
        public double average { get; set; }
        public double max { get; set; }
        public double total { get; set; }
    }

    public class Sqs
    {
        public double average { get; set; }
        public double max { get; set; }
        public double total { get; set; }
    }

    public class Vpc
    {
        public double average { get; set; }
        public double max { get; set; }
        public double total { get; set; }
    }

    public class Stats
    {
        public AWSKeyManagementService AWSKeyManagementService { get; set; }
        public AWSLambda AWSLambda { get; set; }
        public AmazonCloudWatch AmazonCloudWatch { get; set; }
        public Aggregated aggregated { get; set; }
        public Cloudfront cloudfront { get; set; }
        public Cloudwatch cloudwatch { get; set; }
        public Dynamodb dynamodb { get; set; }
        public Ebs ebs { get; set; }
        public Ec2 ec2 { get; set; }
        public Ec2Instance ec2_instance { get; set; }
        public Eip eip { get; set; }
        public Elasticache elasticache { get; set; }
        public Rds rds { get; set; }
        public Route53 route53 { get; set; }
        public S3 s3 { get; set; }
        public Ses ses { get; set; }
        public Simpledb simpledb { get; set; }
        public Sns sns { get; set; }
        public Sqs sqs { get; set; }
        public Vpc vpc { get; set; }
    }
}