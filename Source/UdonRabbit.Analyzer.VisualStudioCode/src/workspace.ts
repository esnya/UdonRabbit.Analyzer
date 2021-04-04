import type { WorkspaceFoldersChangeEvent } from "vscode";

import { writeConfiguration } from "./omnisharp";

const onChangeWorkspace = (e: WorkspaceFoldersChangeEvent): void => {
  if (e.added.length > 0) {
    writeConfiguration();
  }
};

export { onChangeWorkspace };
