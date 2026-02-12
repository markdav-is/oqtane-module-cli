Feature: Command Line Parsing
  As a developer
  I want to use the CLI with various command-line options
  So that I can create Oqtane modules with different configurations

  Background:
    Given I am in an Oqtane solution directory

  Scenario: Create module with required parameters only
    When I run the command "module create --owner TestOwner --name TestModule"
    Then the command should succeed
    And the output should contain "Owner:     TestOwner"
    And the output should contain "Module:    TestModule"
    And the output should contain "Framework: net10.0"

  Scenario: Create module with all parameters
    When I run the command "module create --owner Acme --name Blog --template internal --description 'A blog module' --framework net9.0"
    Then the command should succeed
    And the output should contain "Owner:     Acme"
    And the output should contain "Module:    Blog"
    And the output should contain "Framework: net9.0"
    And the output should contain "Description: A blog module"

  Scenario: Reject non-alphanumeric owner name
    When I run the command "module create --owner Test-Owner --name TestModule"
    Then the command should fail
    And the error output should contain "Owner name must be alphanumeric"

  Scenario: Reject non-alphanumeric module name
    When I run the command "module create --owner TestOwner --name Test-Module"
    Then the command should fail
    And the error output should contain "Module name must be alphanumeric"

  Scenario: Require owner parameter
    When I run the command "module create --name TestModule"
    Then the command should fail
    And the error output should contain "owner"

  Scenario: Require name parameter
    When I run the command "module create --owner TestOwner"
    Then the command should fail
    And the error output should contain "name"

  Scenario: Handle help command for module create
    When I run the command "module create --help"
    Then the command should succeed
    And the output should contain "Create a new Oqtane module"
    And the output should contain "--owner"
    And the output should contain "--name"
    And the output should contain "--template"
