using Xunit;

// WebApplicationFactory<Program> boots a new host per test class by invoking the entry point via
// reflection (HostFactoryResolver). That interception is not safe under concurrent Main
// invocations — running test classes in parallel races two hosts' Build() calls against the same
// diagnostic listener, intermittently producing "entry point exited without ever building an
// IHost". Serilog's extra host-building work (config binding, sink construction) widens this race
// window enough to make it a real problem, so integration tests run sequentially.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
