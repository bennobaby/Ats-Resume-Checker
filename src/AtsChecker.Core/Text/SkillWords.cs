namespace AtsChecker.Core;

public static class SkillWords
{
    public static readonly HashSet<string> Words = new(StringComparer.OrdinalIgnoreCase)
    {
        "python", "java", "javascript", "js", "typescript", "c#", "c++", "f#",
        "kubernetes", "k8s", "postgresql", "postgres", "sql", "mysql", "react",
        "angular", "vue", "node", "nodejs", "docker", "aws", "azure", "gcp",
        "git", "linux", "html", "css", "golang", "rust", "ruby", "php", "scala",
        "kotlin", "swift", "django", "flask", "spring", "dotnet", "net",
        "terraform", "ansible", "jenkins", "graphql", "excel", "tableau",
        "spark", "hadoop", "pandas", "numpy", "tensorflow", "pytorch", "figma",
        "jira", "agile", "scrum"
    };
}
