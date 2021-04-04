// The module 'vscode' contains the VS Code extensibility API
// Import the module and reference it with the alias vscode in your code below
import * as vscode from "vscode";

import { configureExtensionPath, writeConfiguration } from "./omnisharp";
import { onChangeWorkspace } from "./workspace";

// this method is called when your extension is activated
// your extension is activated the very first time the command is executed
export function activate(context: vscode.ExtensionContext) {
  const shouldRewriteOmniSharp = vscode.workspace.getConfiguration("udonrabbit-analyzer").get<boolean>("rewriteOmniSharpJson");

  if (shouldRewriteOmniSharp) {
    configureExtensionPath(context.extensionPath);
    writeConfiguration();

    context.subscriptions.push(vscode.workspace.onDidChangeWorkspaceFolders((e) => onChangeWorkspace(e)));
  }
}

// this method is called when your extension is deactivated
export function deactivate() {}
