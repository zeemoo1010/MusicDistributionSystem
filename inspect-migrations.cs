using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Migrations;

var assemblyPath = @"C:\Projects\MVC\MusicDistributionSystem\MusicDistributionSystem\bin\Debug\net9.0\MusicDistributionSystem.dll";
var assembly = Assembly.LoadFrom(assemblyPath);
var migrationTypes = assembly.GetTypes()
    .Where(t => typeof(Migration).IsAssignableFrom(t) && !t.IsAbstract)
    .Select(t => t.FullName)
    .OrderBy(n => n)
    .ToList();

Console.WriteLine($"Assembly: {Path.GetFileName(assemblyPath)}");
Console.WriteLine($"MigrationCount: {migrationTypes.Count}");
foreach (var name in migrationTypes)
{
    Console.WriteLine(name);
}
