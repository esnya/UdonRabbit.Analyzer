import * as fs from "fs";
import * as json from "json5";
import * as os from "os";
import { dirname, join } from "path";
import { window, workspace } from "vscode";

/* eslint-disable @typescript-eslint/naming-convention */
type OmniSharp = {
  RoslynExtensionsOptions?: {
    EnableAnalyzersSupport?: boolean;
    LocationPaths?: string[];
  };
};
/* eslint-enable @typescript-eslint/naming-convention */

let extensionPath: string;

const configureExtensionPath = (path: string): void => {
  extensionPath = path;
};

const readOmniSharpConfiguration = (path: string): OmniSharp => {
  if (!fs.existsSync(path)) {
    return {} as OmniSharp;
  }

  const content = fs.readFileSync(path, { encoding: "utf8" });
  return json.parse<OmniSharp>(content);
};

const enableOnWorkspace = (path: string): void => {
  const configuration = readOmniSharpConfiguration(path);
  let isUpdated = false;

  if (configuration.RoslynExtensionsOptions === undefined) {
    configuration.RoslynExtensionsOptions = {};
  }

  if (configuration.RoslynExtensionsOptions.EnableAnalyzersSupport !== true) {
    configuration.RoslynExtensionsOptions.EnableAnalyzersSupport = true;
  }

  if (!Array.isArray(configuration.RoslynExtensionsOptions.LocationPaths)) {
    configuration.RoslynExtensionsOptions.LocationPaths = [];
  }

  const analyzers = join(extensionPath, "externals").replace(/\\/g, "/");

  if (configuration.RoslynExtensionsOptions.LocationPaths.find((w) => w.includes("natsuneko.udonrabbit-analyzer"))) {
    const paths = configuration.RoslynExtensionsOptions.LocationPaths.filter((w) => !w.includes("natsuneko.udonrabbit-analyzer"));
    configuration.RoslynExtensionsOptions.LocationPaths = paths;
  }

  if (!configuration.RoslynExtensionsOptions.LocationPaths.includes(analyzers)) {
    configuration.RoslynExtensionsOptions.LocationPaths.push(analyzers);
    isUpdated = true;
  }

  if (isUpdated) {
    fs.writeFileSync(path, JSON.stringify(configuration, null, 4));
    window.showInformationMessage("omnisharp.json has been updated by UdonRabbit.Analyzer for VS Code");
  }
};

const enableOnGlobal = (path: string): void => {
  const baseDir = dirname(path);
  if (!fs.existsSync(baseDir)) {
    fs.mkdirSync(baseDir);
  }

  enableOnWorkspace(path);
};

const writeConfiguration = (): void => {
  if (workspace.workspaceFolders === undefined || workspace.workspaceFolders.length === 0) {
    return;
  }

  let hasConfigurationInWorkspace = false;
  const workspaces = workspace.workspaceFolders;
  for (const workspace of workspaces) {
    if (workspace.uri.scheme !== "file") {
      continue;
    }

    const configuration = join(workspace.uri.fsPath, "omnisharp.json");
    if (fs.existsSync(configuration)) {
      enableOnWorkspace(configuration);
      hasConfigurationInWorkspace = true;
    }
  }

  if (hasConfigurationInWorkspace) {
    return;
  }

  enableOnGlobal(join(os.homedir(), ".omnisharp", "omnisharp.json"));
};

export { configureExtensionPath, writeConfiguration };
