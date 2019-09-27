using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

//	#!/usr/bin/perl
//	#                       
//	# IOMeter results parser, 2005, Paul Venezia
//	#               
//	#       Simply run this script in the directory containing the CSV output of IOMeter.
//	#       Results are organized by filename.
//	#
//	#
//
//
//	use File::Glob ':glob';
//	use Data::Dumper;
//
//	@files = <*.csv>;
//	my %results;
//	my $parmcount = 1;
//	my $lcount = 0;
//	foreach my $file (@files) {
//		undef @lines;
//		open (LOG, "<$file");
//		my @lines = <LOG>;
//		foreach my $line (@lines) {
//			$lcount++;
//			if ($line =~ /'Access\sspecification\sname.+/) {
//				$line = @lines[$lcount];
//				$line =~ /(.+),\d/;
//				#%results->{$file}->{test} = "$1";
//				$thistest = "$1";
//				#print $thistest;
//
//			}
//			if ($line =~ /ALL,All.+/) {
//				#$line = @lines[$lcount];
//				@thisres = (split(",", $line));
//				%results->{$file}->{$thistest}{IOps} = @thisres[6];
//				%results->{$file}->{$thistest}{ReadIOps} = @thisres[7];
//				%results->{$file}->{$thistest}{WriteIOps} = @thisres[8];
//				%results->{$file}->{$thistest}{MBps} = @thisres[9];
//				%results->{$file}->{$thistest}{ReadMBps} = @thisres[10];
//				%results->{$file}->{$thistest}{CPU} = @thisres[45];
//			}
//
//		}			
//	$parmcount = 0;
//	$lcount = 0;
//
//	}
//
//	sub swrite {
//	  my $fmt = shift(@_);
//	  $^A = '';
//	  formline($fmt,@_);
//	  return $^A;
//	}
//
//
//		
//	my %output;		
//	foreach my $logfile ( sort keys %results ) {
//		#$TXT .= "$logfile: \n";
//		$header = 1;
//		foreach $test ( sort keys %{$results{$logfile}} ) {
//			foreach $tsize ( sort keys %{$results{$logfile}{$test}} ) {
//				$output{$test}{$logfile}{$tsize} =  swrite("@<<<<<<<<<<<<<<<<<<", $results{$logfile}{$test}{$tsize});
//			}
//			
//		}
//	}
//
//	#print Dumper %results;
//	foreach $testsize ( sort keys %output ) {
//		$TXT .= "$testsize\n";
//		$header = 1;
//		foreach $unit ( sort keys %{$output{$testsize}} ) {
//			if ($header) {
//				$TXT .= "\t\t";
//				foreach $test ( sort keys %{$output{$testsize}{$unit}} ) {
//					$TXT .= swrite("@>>>>>>>>>>>>> @<<<<<<", $test);
//				}
//				$TXT .= "\n";
//				undef $header;
//			}
//			$testunit = $unit;
//			$testunit =~ s/\.csv//;
//			$TXT .= swrite("@<<<<<<<<<<<<<<<<<<<<<<<< @<<<<<", $testunit);
//				 
//			foreach $test ( sort keys %{$output{$testsize}{$unit}} ) {
//				$TXT .= swrite("@<<<<<<<<<<<<< @<<<<<<<", $output{$testsize}{$unit}{$test});
//			}
//			$TXT .= "\n";
//		}
//		$TXT .= "\n";
//	}
//
//	print $TXT;
//	#print Dumper %output;


namespace meter
{
    class Specification
    {
        private List<Entry> entries = new List<Entry>();
        public string Name { get; set; }

        public void AddEntry(String[] results)
        {
            entries.Add(new Entry(results));
        }
    }

    class File
    {
        private Dictionary<String, Specification> tests = new Dictionary<string, Specification>();

        public Specification GetTest(String name)
        {
            if(!tests.ContainsKey(name)) tests.Add(name, new Specification());
            return tests[name];
        }
    }

    class Results
    {
        private Dictionary<String, File> files = new Dictionary<string, File>();

        public File GetFile(String path)
        {
            if(!files.ContainsKey(path)) files.Add(path, new File());
            return files[path];
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var results = new Results();
            var files = Directory.GetFiles("./");
            foreach (string path in files)
            {
                Console.WriteLine("Read file: " + path);
                var file = results.GetFile(path);
                using (StreamReader reader = new StreamReader(path))
                {
                    var content = reader.ReadToEnd();
                    var lines = content.Split('\n');

                    Specification currentSpecification = null;

                    for (var i = 0; i < lines.Length; i++)
                    {
                        var line = lines[i];

                        var match = Regex.Match(line, @"'Access specification name.+", RegexOptions.IgnoreCase);
                        if (match.Success)
                        {
                            line = lines[i + 1];
                            match = Regex.Match(line, @"(.+),\d", RegexOptions.IgnoreCase);
                            if (match.Success)
                            {
                                var name = match.Groups[1].ToString();
                                currentSpecification = file.GetTest(name);
                                currentSpecification.Name = name;
                            }
                        }

                        match = Regex.Match(line, @"ALL,All.+", RegexOptions.IgnoreCase);
                        if (match.Success)
                        {
                            var resources = line.Split(',');
                            currentSpecification?.AddEntry(resources);
                        }
                    }
                }
            }
        }
    }
}